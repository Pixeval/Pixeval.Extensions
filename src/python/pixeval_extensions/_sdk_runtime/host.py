from __future__ import annotations

import ctypes
from abc import abstractmethod
from collections.abc import Sequence

from .. import abi as _abi
from .callbacks import ArrayReturnCallback, StringReturnCallback
from .core import SdkComObject


__all__ = ["ExtensionsHostBase", "set_sdk_version"]

SDK_VERSION = "0.0.0.0"
HostInitializeCallback = _abi.CALLBACK(
    _abi.HRESULT,
    _abi.VOIDP,
    _abi.VOIDP,
    _abi.VOIDP,
    _abi.VOIDP,
    _abi.VOIDP,
)


def set_sdk_version(value: str) -> None:
    global SDK_VERSION
    SDK_VERSION = value


class ExtensionsHostBase(SdkComObject):
    def __init__(
        self,
        *,
        extra_callbacks: Sequence[ctypes._CFuncPtr] = (),
    ) -> None:
        self.culture_name = ""
        self.temp_directory = ""
        self.extension_directory = ""
        self.logger_pointer = 0
        callbacks = [
            StringReturnCallback(self._get_extension_name),
            StringReturnCallback(self._get_author_name),
            StringReturnCallback(self._get_extension_link),
            StringReturnCallback(self._get_help_link),
            StringReturnCallback(self._get_description),
            StringReturnCallback(self._get_sdk_version),
            StringReturnCallback(self._get_version),
            ArrayReturnCallback(self._get_extensions),
            ArrayReturnCallback(self._get_icon),
            HostInitializeCallback(self._initialize),
            *extra_callbacks,
        ]
        super().__init__(
            _abi.InterfaceSet.of(_abi.IID_IUNKNOWN, _abi.IID_EXTENSIONS_HOST),
            callbacks,
        )

    def initialize(self) -> None:
        pass

    @property
    @abstractmethod
    def extension_name(self) -> str:
        raise NotImplementedError

    @property
    @abstractmethod
    def author_name(self) -> str:
        raise NotImplementedError

    @property
    @abstractmethod
    def extension_link(self) -> str:
        raise NotImplementedError

    @property
    @abstractmethod
    def help_link(self) -> str:
        raise NotImplementedError

    @property
    @abstractmethod
    def description(self) -> str:
        raise NotImplementedError

    @property
    def sdk_version(self) -> str:
        return SDK_VERSION

    @property
    @abstractmethod
    def version(self) -> str:
        raise NotImplementedError

    @property
    @abstractmethod
    def extensions(self) -> Sequence[_abi.ComObject]:
        raise NotImplementedError

    @property
    @abstractmethod
    def icon(self) -> bytes | None:
        raise NotImplementedError

    def _get_extension_name(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.extension_name, result)

    def _get_author_name(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.author_name, result)

    def _get_extension_link(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.extension_link, result)

    def _get_help_link(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.help_link, result)

    def _get_description(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.description, result)

    def _get_sdk_version(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.sdk_version, result)

    def _get_version(self, _self: int, result: ctypes.POINTER(_abi.VOIDP)) -> int:
        return _abi.copy_utf16(self.version, result)

    def _get_extensions(
        self,
        _self: int,
        count: ctypes.POINTER(_abi.INT32),
        result: ctypes.POINTER(_abi.VOIDP),
    ) -> int:
        return _abi.copy_pointer_array((extension.export() for extension in self.extensions), count, result)

    def _get_icon(
        self,
        _self: int,
        count: ctypes.POINTER(_abi.INT32),
        result: ctypes.POINTER(_abi.VOIDP),
    ) -> int:
        return _abi.copy_bytes(self.icon or b"", count, result)

    def _initialize(
        self,
        _self: int,
        culture_name: int,
        temp_directory: int,
        extension_directory: int,
        logger: int,
    ) -> int:
        self.culture_name = _abi.read_utf16(culture_name)
        self.temp_directory = _abi.read_utf16(temp_directory)
        self.extension_directory = _abi.read_utf16(extension_directory)
        self.logger_pointer = int(logger or 0)
        self.initialize()
        return _abi.S_OK

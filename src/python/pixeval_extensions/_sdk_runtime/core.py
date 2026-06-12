from __future__ import annotations

import ctypes
from abc import ABC
from collections.abc import Sequence

from .. import abi as _abi


__all__ = ["SdkComObject"]


class SdkComObject(_abi.ComObject, ABC):
    def __init__(self, interfaces: _abi.InterfaceSet, callbacks: Sequence[ctypes._CFuncPtr]) -> None:
        self._interfaces = interfaces
        _abi.ComObject.__init__(
            self,
            [
                _abi.make_query_interface_callback(),
                _abi.make_add_ref_callback(),
                _abi.make_release_callback(),
                *callbacks,
            ],
        )

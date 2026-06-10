from __future__ import annotations

import ctypes

from .types import CALLBACK, DOUBLE, HRESULT, INT32, S_OK, VOIDP


class NativeComPtr:
    def __init__(self, native: int | None):
        self.native = int(native or 0)

    @property
    def has_value(self) -> bool:
        return self.native != 0

    def _slot(self, index: int) -> int:
        vtable = ctypes.cast(self.native, ctypes.POINTER(ctypes.POINTER(VOIDP))).contents
        return int(vtable[index] or 0)

    def call_no_args(self, index: int) -> int:
        if not self.has_value:
            return S_OK
        function = CALLBACK(HRESULT, VOIDP)(self._slot(index))
        return int(function(self.native))


class TaskCompletionSource(NativeComPtr):
    def set_completed(self) -> int:
        return self.call_no_args(3)


class ProgressNotifier(NativeComPtr):
    def progress_changed(self, progress: float) -> int:
        if not self.has_value:
            return S_OK
        function = CALLBACK(HRESULT, VOIDP, DOUBLE)(self._slot(3))
        return int(function(self.native, progress))

    def completed(self) -> int:
        return self.call_no_args(4)


class Stream(NativeComPtr):
    def copy_to(self, destination: "Stream", buffer_size: int = -1) -> int:
        if not self.has_value:
            return S_OK
        function = CALLBACK(HRESULT, VOIDP, VOIDP, INT32)(self._slot(18))
        return int(function(self.native, destination.native, buffer_size))

from __future__ import annotations

import ctypes

from .. import abi as _abi


__all__ = [
    "NoArgsCallback",
    "StringReturnCallback",
    "IntReturnCallback",
    "UIntReturnCallback",
    "DoubleReturnCallback",
    "ArrayReturnCallback",
    "_copy_int",
    "_copy_uint",
    "_copy_double",
    "_complete_task",
]


NoArgsCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP)
StringReturnCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP, ctypes.POINTER(_abi.VOIDP))
IntReturnCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP, ctypes.POINTER(_abi.INT32))
UIntReturnCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP, ctypes.POINTER(_abi.UINT32))
DoubleReturnCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP, ctypes.POINTER(_abi.DOUBLE))
ArrayReturnCallback = _abi.CALLBACK(_abi.HRESULT, _abi.VOIDP, ctypes.POINTER(_abi.INT32), ctypes.POINTER(_abi.VOIDP))


def _copy_int(value: int, result: ctypes.POINTER(_abi.INT32)) -> int:
    if not result:
        return _abi.E_POINTER
    result[0] = value
    return _abi.S_OK


def _copy_uint(value: int, result: ctypes.POINTER(_abi.UINT32)) -> int:
    if not result:
        return _abi.E_POINTER
    result[0] = value
    return _abi.S_OK


def _copy_double(value: float, result: ctypes.POINTER(_abi.DOUBLE)) -> int:
    if not result:
        return _abi.E_POINTER
    result[0] = value
    return _abi.S_OK


def _complete_task(task: int) -> None:
    _abi.TaskCompletionSource(task).set_completed()

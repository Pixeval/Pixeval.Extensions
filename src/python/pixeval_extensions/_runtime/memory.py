from __future__ import annotations

import ctypes
import sys
from collections.abc import Iterable, Sequence

from .types import E_OUTOFMEMORY, E_POINTER, INT32, S_OK, VOIDP


if sys.platform == "win32":
    _ole32 = ctypes.WinDLL("ole32")
    _ole32.CoTaskMemAlloc.argtypes = [ctypes.c_size_t]
    _ole32.CoTaskMemAlloc.restype = VOIDP
    _allocate = _ole32.CoTaskMemAlloc
else:
    _libc = ctypes.CDLL(None)
    _libc.malloc.argtypes = [ctypes.c_size_t]
    _libc.malloc.restype = VOIDP
    _allocate = _libc.malloc


def allocate(size: int) -> int:
    if size <= 0:
        return 0
    pointer = _allocate(size)
    return int(pointer or 0)


def copy_bytes(value: bytes, count_out: ctypes.POINTER(INT32), result_out: ctypes.POINTER(VOIDP)) -> int:
    if not count_out or not result_out:
        return E_POINTER
    count_out[0] = len(value)
    result_out[0] = None
    if not value:
        return S_OK
    pointer = allocate(len(value))
    if pointer == 0:
        count_out[0] = 0
        return E_OUTOFMEMORY
    ctypes.memmove(pointer, value, len(value))
    result_out[0] = pointer
    return S_OK


def copy_utf16(value: str | None, result_out: ctypes.POINTER(VOIDP)) -> int:
    if not result_out:
        return E_POINTER
    result_out[0] = None
    if value is None:
        return S_OK
    data = value.encode("utf-16-le") + b"\x00\x00"
    pointer = allocate(len(data))
    if pointer == 0:
        return E_OUTOFMEMORY
    ctypes.memmove(pointer, data, len(data))
    result_out[0] = pointer
    return S_OK


def copy_pointer_array(values: Iterable[int], count_out: ctypes.POINTER(INT32), result_out: ctypes.POINTER(VOIDP)) -> int:
    if not count_out or not result_out:
        return E_POINTER
    pointers = list(values)
    count_out[0] = len(pointers)
    result_out[0] = None
    if not pointers:
        return S_OK
    size = ctypes.sizeof(VOIDP) * len(pointers)
    array_pointer = allocate(size)
    if array_pointer == 0:
        count_out[0] = 0
        return E_OUTOFMEMORY
    array = (VOIDP * len(pointers)).from_address(array_pointer)
    for index, pointer in enumerate(pointers):
        array[index] = pointer
    result_out[0] = array_pointer
    return S_OK


def copy_utf16_array(values: Sequence[str], count_out: ctypes.POINTER(INT32), result_out: ctypes.POINTER(VOIDP)) -> int:
    pointers: list[int] = []
    for value in values:
        item = VOIDP()
        hr = copy_utf16(value, ctypes.pointer(item))
        if hr != S_OK:
            return hr
        pointers.append(int(item.value or 0))
    return copy_pointer_array(pointers, count_out, result_out)


def copy_int32_array(values: Sequence[int], result_out: ctypes.POINTER(VOIDP)) -> int:
    if not result_out:
        return E_POINTER
    result_out[0] = None
    if not values:
        return S_OK
    size = ctypes.sizeof(INT32) * len(values)
    pointer = allocate(size)
    if pointer == 0:
        return E_OUTOFMEMORY
    array = (INT32 * len(values)).from_address(pointer)
    for index, value in enumerate(values):
        array[index] = value
    result_out[0] = pointer
    return S_OK


def read_utf16(pointer: int | None) -> str:
    if not pointer:
        return ""
    address = int(pointer)
    length = 0
    while True:
        pair = ctypes.string_at(address + length * 2, 2)
        if pair == b"\x00\x00":
            break
        length += 1
    return ctypes.string_at(address, length * 2).decode("utf-16-le")


def read_pointer_array(pointer: int | None, count: int) -> list[int]:
    if not pointer or count <= 0:
        return []
    array = (VOIDP * count).from_address(int(pointer))
    return [int(array[index] or 0) for index in range(count)]


def read_utf16_array(pointer: int | None, count: int) -> list[str]:
    return [read_utf16(item) for item in read_pointer_array(pointer, count)]


def read_int32_array(pointer: int | None, count: int) -> list[int]:
    if not pointer or count <= 0:
        return []
    array = (INT32 * count).from_address(int(pointer))
    return [int(array[index]) for index in range(count)]

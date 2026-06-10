from __future__ import annotations

import ctypes
import sys
import uuid
from enum import IntEnum


def _hresult(value: int) -> int:
    return value - 0x1_0000_0000 if value >= 0x8000_0000 else value


S_OK = 0
E_POINTER = _hresult(0x80004003)
E_NOINTERFACE = _hresult(0x80004002)
E_OUTOFMEMORY = _hresult(0x8007000E)
E_FAIL = _hresult(0x80004005)

CALLBACK = ctypes.WINFUNCTYPE if sys.platform == "win32" else ctypes.CFUNCTYPE

HRESULT = ctypes.c_int32
ULONG = ctypes.c_uint32
INT32 = ctypes.c_int32
INT64 = ctypes.c_int64
UINT32 = ctypes.c_uint32
DOUBLE = ctypes.c_double
VOIDP = ctypes.c_void_p


class SeekOrigin(IntEnum):
    Begin = 0
    Current = 1
    End = 2


class Guid(ctypes.Structure):
    _fields_ = [
        ("data1", ctypes.c_uint32),
        ("data2", ctypes.c_uint16),
        ("data3", ctypes.c_uint16),
        ("data4", ctypes.c_uint8 * 8),
    ]

    @classmethod
    def parse(cls, value: str) -> "Guid":
        parsed = uuid.UUID(value)
        data4 = bytes([parsed.clock_seq_hi_variant, parsed.clock_seq_low]) + parsed.node.to_bytes(6, "big")
        return cls(parsed.time_low, parsed.time_mid, parsed.time_hi_version, (ctypes.c_uint8 * 8)(*data4))

    def canonical(self) -> str:
        data4 = bytes(self.data4)
        value = uuid.UUID(fields=(self.data1, self.data2, self.data3, data4[0], data4[1], int.from_bytes(data4[2:], "big")))
        return str(value).upper()

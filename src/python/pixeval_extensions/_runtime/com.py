from __future__ import annotations

import ctypes
from dataclasses import dataclass
from typing import ClassVar

from .types import CALLBACK, E_NOINTERFACE, E_POINTER, Guid, HRESULT, ULONG, VOIDP


class NativeObject(ctypes.Structure):
    _fields_ = [("vtable", ctypes.POINTER(VOIDP))]


_objects: dict[int, "ComObject"] = {}

QueryInterfaceCallback = CALLBACK(HRESULT, VOIDP, ctypes.POINTER(Guid), ctypes.POINTER(VOIDP))
AddRefCallback = CALLBACK(ULONG, VOIDP)
ReleaseCallback = CALLBACK(ULONG, VOIDP)


@dataclass(frozen=True)
class InterfaceSet:
    values: frozenset[str]

    @classmethod
    def of(cls, *guids: Guid) -> "InterfaceSet":
        return cls(frozenset(guid.canonical() for guid in guids))

    def contains(self, guid: Guid) -> bool:
        return guid.canonical() in self.values


class ComObject:
    _interfaces: ClassVar[InterfaceSet] = InterfaceSet.of()

    def __init__(self, callbacks: list[ctypes._CFuncPtr]):
        self._callbacks = callbacks
        self._vtable = (VOIDP * len(callbacks))(*(ctypes.cast(callback, VOIDP).value for callback in callbacks))
        self._native = NativeObject(ctypes.cast(self._vtable, ctypes.POINTER(VOIDP)))
        self._address = ctypes.addressof(self._native)
        self._references = 1
        _objects[self._address] = self

    @property
    def address(self) -> int:
        return self._address

    def export(self) -> int:
        self.add_ref()
        return self.address

    def add_ref(self) -> int:
        self._references += 1
        return self._references

    def release(self) -> int:
        self._references = max(0, self._references - 1)
        return self._references

    def supports(self, iid: Guid) -> bool:
        return self._interfaces.contains(iid)

    def query_interface(self, iid: Guid, result_out: ctypes.POINTER(VOIDP)) -> int:
        if not result_out:
            return E_POINTER
        result_out[0] = None
        if not self.supports(iid):
            return E_NOINTERFACE
        self.add_ref()
        result_out[0] = self.address
        return 0


def object_from_pointer(self_pointer: int | None) -> ComObject | None:
    if not self_pointer:
        return None
    return _objects.get(int(self_pointer))


def make_query_interface_callback() -> ctypes._CFuncPtr:
    def callback(self_pointer: int, iid_pointer: ctypes.POINTER(Guid), result_out: ctypes.POINTER(VOIDP)) -> int:
        instance = object_from_pointer(self_pointer)
        if instance is None or not iid_pointer:
            if result_out:
                result_out[0] = None
            return E_POINTER
        return instance.query_interface(iid_pointer.contents, result_out)

    return QueryInterfaceCallback(callback)


def make_add_ref_callback() -> ctypes._CFuncPtr:
    def callback(self_pointer: int) -> int:
        instance = object_from_pointer(self_pointer)
        return 0 if instance is None else instance.add_ref()

    return AddRefCallback(callback)


def make_release_callback() -> ctypes._CFuncPtr:
    def callback(self_pointer: int) -> int:
        instance = object_from_pointer(self_pointer)
        return 0 if instance is None else instance.release()

    return ReleaseCallback(callback)

from __future__ import annotations

from dataclasses import dataclass

from ..symbols import Symbol


__all__ = ["EntryMetadata", "read_icon"]


@dataclass(slots=True)
class EntryMetadata:
    icon: Symbol
    label: str
    description: str
    token: str = ""
    placeholder: str | None = None
    description_uri: str | None = None


def read_icon(path: str) -> bytes:
    try:
        with open(path, "rb") as file:
            return file.read()
    except OSError:
        return b""

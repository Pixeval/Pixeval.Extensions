from __future__ import annotations

from collections.abc import Sequence

import pixeval_extensions.abi as abi
from pixeval_extensions import (
    BoolSettingsExtension,
    ExtensionsHostBase,
    IntSettingsExtension,
    StringSettingsExtension,
    Symbol,
    read_icon,
)


class DemoBoolSetting(BoolSettingsExtension):
    def __init__(self) -> None:
        self.value = self.default_value
        super().__init__()

    @property
    def icon(self) -> Symbol:
        return Symbol.ToggleRight

    @property
    def label(self) -> str:
        return "Python Demo Enabled"

    @property
    def description(self) -> str:
        return "Turns the Python demo setting on or off."

    @property
    def token(self) -> str:
        return "python.demo.enabled"

    @property
    def default_value(self) -> bool:
        return True

    def on_value_changed(self, value: bool) -> None:
        self.value = value


class DemoIntSetting(IntSettingsExtension):
    def __init__(self) -> None:
        self.value = self.default_value
        super().__init__()

    @property
    def icon(self) -> Symbol:
        return Symbol.NumberSymbol

    @property
    def label(self) -> str:
        return "Python Demo Level"

    @property
    def description(self) -> str:
        return "Integer setting sample from the Python SDK."

    @property
    def token(self) -> str:
        return "python.demo.level"

    @property
    def placeholder(self) -> str | None:
        return "0 - 10"

    @property
    def default_value(self) -> int:
        return 3

    @property
    def min_value(self) -> int:
        return 0

    @property
    def max_value(self) -> int:
        return 10

    def on_value_changed(self, value: int) -> None:
        self.value = value


class DemoStringSetting(StringSettingsExtension):
    def __init__(self) -> None:
        self.value = self.default_value
        super().__init__()

    @property
    def icon(self) -> Symbol:
        return Symbol.Text

    @property
    def label(self) -> str:
        return "Python Demo Nickname"

    @property
    def description(self) -> str:
        return "String setting sample from the Python SDK."

    @property
    def token(self) -> str:
        return "python.demo.nickname"

    @property
    def placeholder(self) -> str | None:
        return "Enter a nickname"

    @property
    def default_value(self) -> str:
        return "Pixeval Python Demo"

    def on_value_changed(self, value: str) -> None:
        self.value = value


class DemoHost(ExtensionsHostBase):
    def __init__(self) -> None:
        self._extensions = [DemoBoolSetting(), DemoIntSetting(), DemoStringSetting()]
        super().__init__()

    @property
    def extension_name(self) -> str:
        return "Pixeval Python SDK Demo"

    @property
    def author_name(self) -> str:
        return "Pixeval.Extensions Python SDK"

    @property
    def extension_link(self) -> str:
        return "https://github.com/Pixeval/Pixeval.Extensions"

    @property
    def help_link(self) -> str:
        return "https://github.com/Pixeval/Pixeval.Extensions/tree/master/src/python"

    @property
    def description(self) -> str:
        return "A Python extension implemented through the native Pixeval Python bootstrap."

    @property
    def version(self) -> str:
        return "0.1.0"

    @property
    def extensions(self) -> Sequence[abi.ComObject]:
        return self._extensions

    @property
    def icon(self) -> bytes | None:
        return read_icon(r"D:\logo.png")


_HOST = DemoHost()


def get_extensions_host() -> int:
    return _HOST.export()

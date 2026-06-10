from __future__ import annotations

from pixeval_extensions import (
    BoolSettingsExtension,
    EntryMetadata,
    ExtensionsHostBase,
    IntSettingsExtension,
    StringSettingsExtension,
    Symbol,
    read_icon,
)


class DemoHost(ExtensionsHostBase):
    def __init__(self) -> None:
        super().__init__(
            extension_name="Pixeval Python SDK Demo",
            author_name="Pixeval.Extensions Python SDK",
            extension_link="https://github.com/Pixeval/Pixeval.Extensions",
            help_link="https://github.com/Pixeval/Pixeval.Extensions/tree/master/src/python",
            description="A Python extension implemented through the native Pixeval Python bootstrap.",
            version="0.1.0",
            icon=read_icon(r"D:\logo.png"),
        )

        self.add_extension(
            BoolSettingsExtension(
                EntryMetadata(
                    icon=Symbol.ToggleRight,
                    label="Python Demo Enabled",
                    description="Turns the Python demo setting on or off.",
                    token="python.demo.enabled",
                ),
                default_value=True,
            )
        )
        self.add_extension(
            IntSettingsExtension(
                EntryMetadata(
                    icon=Symbol.NumberSymbol,
                    label="Python Demo Level",
                    description="Integer setting sample from the Python SDK.",
                    token="python.demo.level",
                    placeholder="0 - 10",
                ),
                default_value=3,
                min_value=0,
                max_value=10,
                step_value=1,
            )
        )
        self.add_extension(
            StringSettingsExtension(
                EntryMetadata(
                    icon=Symbol.Text,
                    label="Python Demo Nickname",
                    description="String setting sample from the Python SDK.",
                    token="python.demo.nickname",
                    placeholder="Enter a nickname",
                ),
                default_value="Pixeval Python Demo",
            )
        )


_HOST = DemoHost()


def dll_get_extensions_host() -> int:
    return _HOST.export()

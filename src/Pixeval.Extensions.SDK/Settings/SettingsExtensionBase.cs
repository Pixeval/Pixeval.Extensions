// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("CE65064F-58B5-4169-98DD-1B5C7F1A538D")]
public abstract partial class SettingsExtensionBase : ISettingsExtension
{
    public abstract Symbol Symbol { get; }

    public abstract string Name { get; }

    public abstract string Header { get; }

    public abstract string Description { get; }

    public abstract SettingsType SettingsType { get; }

    /// <inheritdoc />
    public Symbol GetSymbol() => Symbol;

    /// <inheritdoc />
    string ISettingsExtension.GetName() => Name;

    /// <inheritdoc />
    string ISettingsExtension.GetHeader() => Header;

    /// <inheritdoc />
    string ISettingsExtension.GetDescription() => Description;

    /// <inheritdoc />
    SettingsType ISettingsExtension.GetSettingsType() => SettingsType;

    /// <inheritdoc cref="IExtension.OnExtensionLoaded" />
    public abstract void OnExtensionLoaded();

    /// <inheritdoc cref="IExtension.OnExtensionLoaded" />
    public abstract void OnExtensionUnloaded();
}

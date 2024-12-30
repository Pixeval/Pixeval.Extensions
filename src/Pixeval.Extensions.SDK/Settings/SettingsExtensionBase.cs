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
    public abstract Symbol Icon { get; }

    public abstract string Token { get; }

    public abstract string Label { get; }

    public abstract string Description { get; }

    public virtual string? DescriptionUri => null;

    public abstract SettingsType SettingsType { get; }

    /// <inheritdoc />
    Symbol IEntryExtension.GetIcon() => Icon;

    /// <inheritdoc />
    string IEntryExtension.GetLabel() => Label;

    /// <inheritdoc />
    string IEntryExtension.GetDescription() => Description;

    /// <inheritdoc />
    string? ISettingsExtension.GetDescriptionUri() => DescriptionUri;

    /// <inheritdoc />
    string ISettingsExtension.GetToken() => Token;

    /// <inheritdoc />
    SettingsType ISettingsExtension.GetSettingsType() => SettingsType;

    /// <inheritdoc cref="IExtension.OnExtensionLoaded" />
    public abstract void OnExtensionLoaded();

    /// <inheritdoc cref="IExtension.OnExtensionLoaded" />
    public abstract void OnExtensionUnloaded();
}

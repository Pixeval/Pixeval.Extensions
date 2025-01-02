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
public abstract partial class SettingsExtensionBase : ExtensionBase, ISettingsExtension
{
    /// <inheritdoc cref="IEntryExtension.GetIcon" />
    public abstract Symbol Icon { get; }

    /// <inheritdoc cref="IEntryExtension.GetLabel" />
    public abstract string Label { get; }

    /// <inheritdoc cref="IEntryExtension.GetDescription" />
    public abstract string Description { get; }

    /// <inheritdoc cref="ISettingsExtension.GetToken" />
    public abstract string Token { get; }

    /// <inheritdoc cref="ISettingsExtension.GetDescriptionUri" />
    public virtual string? DescriptionUri => null;

    /// <inheritdoc cref="ISettingsExtension.GetSettingsType" />
    public abstract SettingsType SettingsType { get; }

    /// <inheritdoc />
    Symbol IEntryExtension.GetIcon() => Icon;

    /// <inheritdoc />
    string IEntryExtension.GetLabel() => Label;

    /// <inheritdoc />
    string IEntryExtension.GetDescription() => Description;

    /// <inheritdoc />
    string ISettingsExtension.GetToken() => Token;

    /// <inheritdoc />
    string? ISettingsExtension.GetDescriptionUri() => DescriptionUri;

    /// <inheritdoc />
    SettingsType ISettingsExtension.GetSettingsType() => SettingsType;
}

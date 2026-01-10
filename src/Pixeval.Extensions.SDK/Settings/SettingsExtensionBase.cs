// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="ISettingsExtension" />
[GeneratedComClass]
public abstract partial class SettingsExtensionBase : EntryExtensionBase, ISettingsExtension
{
    /// <inheritdoc cref="ISettingsExtension.GetToken" />
    public abstract string Token { get; }

    /// <inheritdoc cref="ISettingsExtension.GetDescriptionUri" />
    public virtual string? DescriptionUri => null;

    /// <inheritdoc cref="ISettingsExtension.GetPlaceholder" />
    public virtual string? Placeholder => null;

    /// <inheritdoc cref="ISettingsExtension.GetSettingsType" />
    public abstract SettingsType SettingsType { get; }

    /// <inheritdoc />
    string ISettingsExtension.GetToken() => Token;

    /// <inheritdoc />
    public string? GetPlaceholder() => Placeholder;

    /// <inheritdoc />
    string? ISettingsExtension.GetDescriptionUri() => DescriptionUri;

    /// <inheritdoc />
    SettingsType ISettingsExtension.GetSettingsType() => SettingsType;
}

// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions.SDK;

/// <inheritdoc cref="IEntryExtension" />
[GeneratedComClass]
public abstract partial class EntryExtensionBase : ExtensionBase, IEntryExtension
{
    /// <inheritdoc cref="IEntryExtension.GetIcon" />
    public abstract Symbol Icon { get; }

    /// <inheritdoc cref="IEntryExtension.GetLabel" />
    public abstract string Label { get; }

    /// <inheritdoc cref="IEntryExtension.GetDescription" />
    public abstract string Description { get; }

    /// <inheritdoc />
    Symbol IEntryExtension.GetIcon() => Icon;

    /// <inheritdoc />
    string IEntryExtension.GetLabel() => Label;

    /// <inheritdoc />
    string IEntryExtension.GetDescription() => Description;
}

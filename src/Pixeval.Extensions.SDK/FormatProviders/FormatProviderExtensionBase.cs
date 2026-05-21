using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class FormatProviderExtensionBase : EntryExtensionBase, IFormatProviderExtension
{
    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatExtension() => FormatExtension;

    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatDescription() => FormatDescription;

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatExtension"/>
    public abstract string FormatExtension { get; }

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatDescription"/>
    public abstract string FormatDescription { get; }

    /// <inheritdoc />
    public override Symbol Icon => Symbol.Document;

    /// <inheritdoc />
    public override string Label => FormatDescription;

    /// <inheritdoc />
    public override string Description => FormatDescription;
}

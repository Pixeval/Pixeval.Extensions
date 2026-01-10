using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class FormatProviderExtensionBase : ExtensionBase, IFormatProviderExtension
{
    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatExtension() => FormatExtension;

    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatDescription() => FormatDescription;

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatExtension"/>
    public abstract string FormatExtension { get; }

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatDescription"/>
    public abstract string FormatDescription { get; }
}

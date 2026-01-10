using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.FormatProviders;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("B9086099-C91F-4432-97E7-C219D5C340DF")]
public partial interface IFormatProviderExtension : IExtension
{
    /// <summary>
    /// Format extension. Including the leading dot. (e.g. ".pdf", ".webp")
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetFormatExtension();

    /// <summary>
    /// Describe the new format.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetFormatDescription();
}

public static partial class FormatProviderExtensionHelper
{
    extension(IFormatProviderExtension extension)
    {
        /// <inheritdoc cref="IFormatProviderExtension.GetFormatExtension"/>
        public string FormatExtension => extension.GetFormatExtension();

        /// <inheritdoc cref="IFormatProviderExtension.GetFormatDescription"/>
        public string FormatDescription => extension.GetFormatDescription();
    }
}

using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace Pixeval.Extensions.Common.FormatProviders;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("B9086099-C91F-4432-97E7-C219D5C340DF")]
public partial interface IFormatProviderExtension : IExtension
{
    /// <summary>
    /// Format extension. Including the leading dot. (e.g. ".pdf", ".webp")
    /// </summary>
    string GetFormatExtension();

    /// <summary>
    /// Describe the new format.
    /// </summary>
    string GetFormatDescription();

    /// <summary>
    /// Get the exception message if any exception occurred during the formatting process.
    /// </summary>
    string? GetFormatExceptionMessage();
}

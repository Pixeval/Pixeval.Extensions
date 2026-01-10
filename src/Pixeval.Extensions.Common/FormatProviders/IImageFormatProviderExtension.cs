using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.FormatProviders;

/// <summary>
/// Extension for a new format for downloads of images.
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("48376829-2728-4918-93C4-3A12955F57D6")]
public partial interface IImageFormatProviderExtension : IFormatProviderExtension
{
    /// <summary>
    /// Format the novel into this format.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="imageStream">Original image stream</param>
    /// <param name="destinationStream">Destination formatted image stream</param>
    void FormatImage(ITaskCompletionSource task, IStream imageStream, IStream destinationStream);
}

public static partial class FormatProviderExtensionHelper
{
    extension(IImageFormatProviderExtension extension)
    {
        /// <inheritdoc cref="IImageFormatProviderExtension.FormatImage"/>
        public async Task FormatImageAsync(Stream imageStream, Stream destinationStream)
        {
            var source = new TaskCompletionSource();
            extension.FormatImage(source.ToITaskCompletionSource(), imageStream.ToIStream(), destinationStream.ToIStream());
            await source.Task;
        }
    }
}

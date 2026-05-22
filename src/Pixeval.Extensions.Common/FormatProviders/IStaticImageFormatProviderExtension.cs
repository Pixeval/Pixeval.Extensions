using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.FormatProviders;

/// <summary>
/// Extension for a new format for downloads of static images.
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("63F6B783-91B6-4F2A-9F6C-2F8A2EF3D24C")]
public partial interface IStaticImageFormatProviderExtension : IFormatProviderExtension
{
    /// <summary>
    /// Format a static image into this format.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="imageStream">Original image stream.</param>
    /// <param name="destinationPath">Destination path.</param>
    void FormatImage(
        ITaskCompletionSource task,
        IStream imageStream,
        string destinationPath);
}

public static partial class FormatProviderExtensionHelper
{
    extension(IStaticImageFormatProviderExtension extension)
    {
        /// <inheritdoc cref="IStaticImageFormatProviderExtension.FormatImage"/>
        public async Task FormatImageAsync(Stream imageStream, string destinationPath)
        {
            var source = new TaskCompletionSource();
            extension.FormatImage(source.ToITaskCompletionSource(), imageStream.ToIStream(), destinationPath);
            await source.Task;
        }
    }
}
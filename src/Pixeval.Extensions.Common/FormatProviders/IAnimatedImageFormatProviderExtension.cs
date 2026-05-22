using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.FormatProviders;

/// <summary>
/// Extension for a new format for downloads of animated images.
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("DFD5291A-CC47-4A2D-A8E5-78B41611A9BC")]
public partial interface IAnimatedImageFormatProviderExtension : IFormatProviderExtension
{
    /// <summary>
    /// Format image streams into this format.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="imageStreams">Original image streams.</param>
    /// <param name="delays">Frame delays in milliseconds.</param>
    /// <param name="count">Count of image streams.</param>
    /// <param name="delayCount">Count of frame delays.</param>
    /// <param name="destinationPath">Destination path.</param>
    void FormatImage(
        ITaskCompletionSource task,
        [MarshalUsing(CountElementName = nameof(count))] IStream[] imageStreams,
        [MarshalUsing(CountElementName = nameof(delayCount))] int[] delays,
        int count,
        int delayCount,
        string destinationPath);
}

public static partial class FormatProviderExtensionHelper
{
    extension(IAnimatedImageFormatProviderExtension extension)
    {
        /// <inheritdoc cref="IAnimatedImageFormatProviderExtension.FormatImage"/>
        public async Task FormatImageAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath)
        {
            var source = new TaskCompletionSource();
            var streams = new IStream[imageStreams.Count];
            for (var i = 0; i < imageStreams.Count; i++)
                streams[i] = imageStreams[i].ToIStream();

            extension.FormatImage(source.ToITaskCompletionSource(), streams, [.. delays], streams.Length, delays.Count, destinationPath);
            await source.Task;
        }
    }
}
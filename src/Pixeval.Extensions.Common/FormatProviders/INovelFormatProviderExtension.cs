// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.FormatProviders;

/// <summary>
/// Extension for a new format for downloads of Pixiv novels.
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("76555005-CF80-4349-B18D-9BCA01E90E3B")]
public partial interface INovelFormatProviderExtension : IFormatProviderExtension
{
    /// <summary>
    /// Format the novel into this format.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="novelInput">Pixiv novel original source</param>
    /// <param name="destinationPath">Destination path</param>
    /// <param name="imageNames">Downloaded image file names used by the novel.</param>
    /// <param name="images">Downloaded image streams used by the novel.</param>
    /// <param name="count">Count of downloaded images.</param>
    void FormatNovel(
        ITaskCompletionSource task,
        string novelInput,
        string destinationPath,
        [MarshalUsing(CountElementName = nameof(count))] string[] imageNames,
        [MarshalUsing(CountElementName = nameof(count))] IStream[] images,
        int count);
}

public static partial class FormatProviderExtensionHelper
{
    extension(INovelFormatProviderExtension extension)
    {
        /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
        public async Task FormatNovelAsync(string novelInput, string destinationPath, IReadOnlyDictionary<string, Stream> images)
        {
            var source = new TaskCompletionSource();
            var imageNames = new string[images.Count];
            var imageStreams = new IStream[images.Count];
            var i = 0;
            foreach (var (imageName, imageStream) in images)
            {
                imageNames[i] = imageName;
                imageStreams[i] = imageStream.ToIStream();
                ++i;
            }

            extension.FormatNovel(source.ToITaskCompletionSource(), novelInput, destinationPath, imageNames, imageStreams, images.Count);
            await source.Task;
        }
    }
}

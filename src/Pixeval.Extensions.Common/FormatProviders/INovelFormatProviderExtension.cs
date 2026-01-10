// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

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
    /// <param name="tempImagePath">Temporary images' path. You can read the images based on the filename and insert them into the generated novel file</param>
    void FormatNovel(ITaskCompletionSource task, string novelInput, string destinationPath, string tempImagePath);
}

public static partial class FormatProviderExtensionHelper
{
    extension(INovelFormatProviderExtension extension)
    {
        /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
        public async Task FormatNovelAsync(string novelInput, string destinationPath, string tempImagePath)
        {
            var source = new TaskCompletionSource();
            extension.FormatNovel(source.ToITaskCompletionSource(), novelInput, destinationPath, tempImagePath);
            await source.Task;
        }
    }
}

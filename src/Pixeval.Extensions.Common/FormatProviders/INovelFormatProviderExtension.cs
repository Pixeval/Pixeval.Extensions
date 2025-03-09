// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common.Internal;

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
    /// <param name="destination">Destination path</param>
    /// <param name="tempImagePath">Temporary images' path. You can read the images based on the filename and insert them into the generated novel file</param>
    void FormatNovel(ITaskCompletionSource task, string novelInput, string destination, string tempImagePath);
}

public static class NovelFormatProviderExtensionHelper
{
    /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
    public static async Task FormatNovelAsync(this INovelFormatProviderExtension extension, string novelInput, string destination, string tempImagePath)
    {
        var wrapper = new TaskCompletionSourceWrapper(new());
        extension.FormatNovel(wrapper, novelInput, destination, tempImagePath);
        await wrapper.Task;
        if (extension.GetFormatExceptionMessage() is { } result)
            ThrowHelper.Format(result);
    }
}

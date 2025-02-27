// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Internal;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Downloaders;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("848517E4-6CA0-40CE-9F8E-3C0E2108284A")]
public partial interface IImageDownloaderExtension
{
    void Download(ITaskCompletionSource task, string uri, string destination);

    string? GetDownloadExceptionMessage();
}

public static class ImageDownloaderExtensionHelper
{
    /// <inheritdoc cref="IImageDownloaderExtension.Download"/>
    public static async Task DownloadAsync(this IImageDownloaderExtension extension, string uri, string destination)
    {
        var wrapper = new TaskCompletionSourceWrapper(new());
        extension.Download(wrapper, uri, destination);
        await wrapper.Task;
        if (extension.GetDownloadExceptionMessage() is { } result)
            ThrowHelper.HttpRequest(result);
    }
}

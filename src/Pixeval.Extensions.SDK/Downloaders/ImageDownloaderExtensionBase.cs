// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Downloaders;

namespace Pixeval.Extensions.SDK.Downloaders;

/// <inheritdoc cref="IImageDownloaderExtension" />
[GeneratedComClass]
public abstract partial class ImageDownloaderExtensionBase : IImageDownloaderExtension
{
    private string? _exceptionMessage;

    /// <inheritdoc />
    async void IImageDownloaderExtension.Download(ITaskCompletionSource task, string uri, string destination)
    {
        try
        {
            _exceptionMessage = await DownloadAsync(uri, destination);
        }
        catch (Exception e)
        {
            _exceptionMessage = e.Message;
        }
    }

    /// <inheritdoc />
    string? IImageDownloaderExtension.GetDownloadExceptionMessage() => _exceptionMessage;

    /// <inheritdoc cref="IImageDownloaderExtension.Download" />
    public abstract Task<string?> DownloadAsync(string uri, string destination);
}

using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IImageFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class ImageFormatProviderExtensionBase : FormatProviderExtensionBase, IImageFormatProviderExtension
{
    /// <inheritdoc />
    async void IImageFormatProviderExtension.FormatImage(ITaskCompletionSource task, IStream imageStream, IStream destinationStream)
    {
        try
        {
            await FormatImageAsync(imageStream, destinationStream);
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="IImageFormatProviderExtension.FormatImage"/>
    public abstract Task FormatImageAsync(IStream imageStream, IStream destinationStream);
}

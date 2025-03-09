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
        var completed = false;
        var exceptionString = "";
        try
        {
            ExceptionMessage = await FormatImageAsync(imageStream, destinationStream);
            if (ExceptionMessage is null)
            {
                task.SetCompleted();
                completed = true;
            }
        }
        catch (Exception e)
        {
            exceptionString = e.Message;
        }
        finally
        {
            if (!completed)
                task.SetException(exceptionString);
        }
    }

    /// <inheritdoc cref="IImageFormatProviderExtension.FormatImage"/>
    public abstract Task<string?> FormatImageAsync(IStream imageStream, IStream destinationStream);
}

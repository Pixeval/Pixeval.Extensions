using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IStaticImageFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class StaticImageFormatProviderExtensionBase : FormatProviderExtensionBase, IStaticImageFormatProviderExtension
{
    /// <inheritdoc />
    async void IStaticImageFormatProviderExtension.FormatImage(
        ITaskCompletionSource task,
        IStream imageStream,
        string destinationPath)
    {
        try
        {
            await FormatImageAsync(imageStream.ToStream(), destinationPath);
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="IStaticImageFormatProviderExtension.FormatImage"/>
    public abstract Task FormatImageAsync(Stream imageStream, string destinationPath);
}
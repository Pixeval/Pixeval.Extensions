using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;

namespace Pixeval.Extensions.SDK.Transformers;

/// <inheritdoc cref="IImageTransformerCommandExtension" />
[GeneratedComClass]
public abstract partial class ImageTransformerCommandExtensionBase : EntryExtensionBase, IImageTransformerCommandExtension
{
    /// <inheritdoc />
    async void IImageTransformerCommandExtension.Transform(ITaskCompletionSource task, IStream originalStream, IStream destinationStream)
    {
        try
        {
            await TransformAsync(originalStream.ToStream(), destinationStream.ToStream());
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="IImageTransformerCommandExtension.Transform" />
    public abstract Task TransformAsync(Stream originalStream, Stream destinationStream);
}

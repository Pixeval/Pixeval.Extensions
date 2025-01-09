using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;

namespace Pixeval.Extensions.SDK.Transformers;

/// <inheritdoc cref="IImageTransformerCommandExtension" />
[GeneratedComClass]
public abstract partial class ImageTransformerExtensionBase : EntryExtensionBase, IImageTransformerCommandExtension
{
    private IStream? _transformResult;

    /// <inheritdoc />
    async void IImageTransformerCommandExtension.Transform(ITaskCompletionSource task, IStream originalStream)
    {
        var completed = false;
        var exceptionString = "";
        try
        {
            if (await TransformAsync(originalStream) is { } result)
            {
                _transformResult = result;
                task.SetCompleted();
                completed = true;
            }
            else
                exceptionString = "result is null";
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

    /// <inheritdoc />
    IStream? IImageTransformerCommandExtension.GetTransformResult() => _transformResult;

    /// <inheritdoc cref="IImageTransformerCommandExtension.Transform" />
    public abstract Task<IStream?> TransformAsync(IStream originalStream);
}

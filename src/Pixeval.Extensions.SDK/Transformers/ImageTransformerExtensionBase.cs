using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Transformers;

namespace Pixeval.Extensions.SDK.Transformers;

[GeneratedComClass]
[Guid("88D897C3-94B7-4739-A821-013C2E4FA0B8")]
public abstract partial class ImageTransformerExtensionBase : IImageTransformerExtension
{
    private IStream? _transformResult;

    async void IImageTransformerExtension.Transform(ITaskCompletionSource task, IStream originalStream)
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

    IStream? IImageTransformerExtension.GetTransformResult() => _transformResult;

    public abstract Task<IStream?> TransformAsync(IStream originalStream);

    public abstract void OnExtensionLoaded();

    public abstract void OnExtensionUnloaded();
}

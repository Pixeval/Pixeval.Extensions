// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common.Internal;

namespace Pixeval.Extensions.Common.Transformers;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3C330C19-8DC1-4180-B309-D446139D387D")]
public partial interface IImageTransformerExtension : IExtension
{
     void Transform(ITaskCompletionSource task, IStream originalStream);

     IStream? GetTransformResult();
}

public static class ImageTransformerExtensionHelper
{
    public static async Task<IStream?> TransformAsync(this IImageTransformerExtension extension, IStream originalStream)
    {
        var wrapper = new TaskCompletionSourceWrapper(new());
        extension.Transform(wrapper, originalStream);
        await wrapper.Task;
        return extension.GetTransformResult();
    }
}

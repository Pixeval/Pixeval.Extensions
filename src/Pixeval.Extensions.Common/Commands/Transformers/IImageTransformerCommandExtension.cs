// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Commands.Transformers;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3C330C19-8DC1-4180-B309-D446139D387D")]
public partial interface IImageTransformerCommandExtension : IViewerCommandExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    void Transform(ITaskCompletionSource task, IStream originalStream, IStream destinationStream);
}

public static partial class TransformerCommandExtensionHelper
{
    extension(IImageTransformerCommandExtension extension)
    {
        /// <inheritdoc cref="IImageTransformerCommandExtension.Transform"/>
        public async Task TransformAsync(Stream originalStream, Stream destinationStream)
        {
            var source = new TaskCompletionSource();
            extension.Transform(source.ToITaskCompletionSource(), originalStream.ToIStream(), destinationStream.ToIStream());
            await source.Task;
        }
    }
}

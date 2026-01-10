// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Commands.Transformers;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("49BE742F-D551-48A6-A32F-6A05E85EB2CD")]
public partial interface ITextTransformerCommandExtension : IViewerCommandExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    void Transform(ITaskCompletionSource task, string originalString, TextTransformerType type);

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetTransformResult();
}

public static partial class TransformerCommandExtensionHelper
{
    extension(ITextTransformerCommandExtension extension)
    {
        /// <inheritdoc cref="ITextTransformerCommandExtension.Transform"/>
        public async Task<string?> TransformAsync(string originalString, TextTransformerType type)
        {
            var source = new TaskCompletionSource();
            extension.Transform(source.ToITaskCompletionSource(), originalString, type);
            await source.Task;
            return extension.GetTransformResult();
        }
    }
}

public enum TextTransformerType
{
    WorkTitle,
    WorkCaption,
    WorkTag,
    UserComment,
    Comment,
    Novel
}

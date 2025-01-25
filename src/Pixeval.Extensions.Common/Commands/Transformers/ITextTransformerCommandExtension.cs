// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Internal;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Commands.Transformers;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("49BE742F-D551-48A6-A32F-6A05E85EB2CD")]
public partial interface ITextTransformerCommandExtension : IViewerCommandExtension
{
    void Transform(ITaskCompletionSource task, string originalString, TextTransformerType type);

    string? GetTransformResult();
}

public static class TextTransformerCommandExtensionHelper
{
    public static async Task<string?> TransformAsync(this ITextTransformerCommandExtension extension, string originalString, TextTransformerType type)
    {
        var wrapper = new TaskCompletionSourceWrapper(new());
        extension.Transform(wrapper, originalString, type);
        await wrapper.Task;
        return extension.GetTransformResult();
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

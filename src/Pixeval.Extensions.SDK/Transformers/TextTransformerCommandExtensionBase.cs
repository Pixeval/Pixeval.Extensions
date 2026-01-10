// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.SDK.Transformers;

/// <inheritdoc cref="ITextTransformerCommandExtension" />
[GeneratedComClass]
public abstract partial class TextTransformerCommandExtensionBase : EntryExtensionBase, ITextTransformerCommandExtension
{
    private string _transformResult = "";

    /// <inheritdoc />
    async void ITextTransformerCommandExtension.Transform(ITaskCompletionSource task, string originalString, TextTransformerType type)
    {
        try
        {
            _transformResult = await TransformAsync(originalString, type);
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc />
    string ITextTransformerCommandExtension.GetTransformResult() => _transformResult;

    /// <inheritdoc cref="ITextTransformerCommandExtension.Transform" />
    public abstract Task<string> TransformAsync(string originalStream, TextTransformerType type);
}

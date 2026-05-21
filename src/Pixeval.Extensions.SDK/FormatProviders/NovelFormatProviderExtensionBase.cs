using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="INovelFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class NovelFormatProviderExtensionBase : FormatProviderExtensionBase, INovelFormatProviderExtension
{
    /// <inheritdoc />
    async void INovelFormatProviderExtension.FormatNovel(
        ITaskCompletionSource task,
        string novelInput,
        string destinationPath,
        string[] imageNames,
        IStream[] images,
        int count)
    {
        try
        {
            await FormatNovelAsync(novelInput, destinationPath, imageNames
                .Take(count)
                .Zip(images.Take(count), static (name, stream) => new KeyValuePair<string, Stream>(name, stream.ToStream()))
                .ToDictionary());
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
    public abstract Task FormatNovelAsync(string novelInput, string destinationPath, IReadOnlyDictionary<string, Stream> images);
}

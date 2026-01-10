using System;
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
    async void INovelFormatProviderExtension.FormatNovel(ITaskCompletionSource task, string novelInput, string destination, string tempImagePath)
    {
        try
        {
            await FormatNovelAsync(novelInput, destination, tempImagePath);
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
    public abstract Task FormatNovelAsync(string novelInput, string destination, string tempImagePath);
}

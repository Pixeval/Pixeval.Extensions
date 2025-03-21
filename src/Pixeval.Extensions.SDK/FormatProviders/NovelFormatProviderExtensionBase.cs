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
        var completed = false;
        var exceptionString = "";
        try
        {
            ExceptionMessage = await FormatNovelAsync(novelInput, destination, tempImagePath);
            if (ExceptionMessage is null)
            {
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

    /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
    public abstract Task<string?> FormatNovelAsync(string novelInput, string destination, string tempImagePath);
}

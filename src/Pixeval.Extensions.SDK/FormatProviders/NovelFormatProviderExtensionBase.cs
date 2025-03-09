using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class FormatProviderExtensionBase : ExtensionBase, IFormatProviderExtension
{
    private protected string? ExceptionMessage;

    /// <inheritdoc />
    string? IFormatProviderExtension.GetFormatExceptionMessage() => ExceptionMessage;

    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatExtension() => FormatExtension;

    /// <inheritdoc />
    string IFormatProviderExtension.GetFormatDescription() => FormatDescription;

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatExtension"/>
    public abstract string FormatExtension { get; }

    /// <inheritdoc cref="IFormatProviderExtension.GetFormatDescription"/>
    public abstract string FormatDescription { get; }
}

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


/// <inheritdoc cref="IImageFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class ImageFormatProviderExtensionBase : FormatProviderExtensionBase, IImageFormatProviderExtension
{
    /// <inheritdoc />
    async void IImageFormatProviderExtension.FormatImage(ITaskCompletionSource task, IStream imageStream, IStream destinationStream)
    {
        var completed = false;
        var exceptionString = "";
        try
        {
            ExceptionMessage = await FormatImageAsync(imageStream, destinationStream);
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

    /// <inheritdoc cref="IImageFormatProviderExtension.FormatImage"/>
    public abstract Task<string?> FormatImageAsync(IStream imageStream, IStream destinationStream);
}

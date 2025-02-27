using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Downloaders;

namespace Pixeval.Extensions.SDK.Downloaders;

/// <inheritdoc cref="INovelFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class NovelFormatProviderExtensionBase : INovelFormatProviderExtension
{
    private string? _exceptionMessage;

    /// <inheritdoc />
    string? INovelFormatProviderExtension.GetFormatExceptionMessage() => _exceptionMessage;

    /// <inheritdoc />
    string INovelFormatProviderExtension.GetFormatExtension() => FormatExtension;

    /// <inheritdoc />
    string INovelFormatProviderExtension.GetFormatDescription() => FormatDescription;

    /// <inheritdoc />
    async void INovelFormatProviderExtension.FormatNovel(ITaskCompletionSource task, string novelInput, string destination, string tempImagePath)
    {
        try
        {
            _exceptionMessage = await FormatNovelAsync(novelInput, destination, tempImagePath);
        }
        catch (Exception e)
        {
            _exceptionMessage = e.Message;
        }
    }

    /// <inheritdoc cref="INovelFormatProviderExtension.GetFormatExtension"/>
    public abstract string FormatExtension { get; }

    /// <inheritdoc cref="INovelFormatProviderExtension.GetFormatDescription"/>
    public abstract string FormatDescription { get; }

    /// <inheritdoc cref="INovelFormatProviderExtension.FormatNovel"/>
    public abstract Task<string?> FormatNovelAsync(string novelInput, string destination, string tempImagePath);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;

namespace Pixeval.Extensions.SDK.FormatProviders;

/// <inheritdoc cref="IAnimatedImageFormatProviderExtension"/>
[GeneratedComClass]
public abstract partial class AnimatedImageFormatProviderExtensionBase : FormatProviderExtensionBase, IAnimatedImageFormatProviderExtension
{
    /// <inheritdoc />
    async void IAnimatedImageFormatProviderExtension.FormatImage(
        ITaskCompletionSource task,
        IStream[] imageStreams,
        int[] delays,
        int count,
        int delayCount,
        string destinationPath)
    {
        try
        {
            await FormatImageAsync(
                [.. imageStreams.Take(count).Select(static stream => stream.ToStream())],
                [.. delays.Take(delayCount)],
                destinationPath);
            task.SetCompleted();
        }
        catch (Exception e)
        {
            task.SetException(e);
        }
    }

    /// <inheritdoc cref="IAnimatedImageFormatProviderExtension.FormatImage"/>
    public abstract Task FormatImageAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath);
}
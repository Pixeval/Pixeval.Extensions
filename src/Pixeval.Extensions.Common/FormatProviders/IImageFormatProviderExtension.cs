using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using Pixeval.Extensions.Common.Internal;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.FormatProviders;

/// <summary>
/// Extension for a new format for downloads of images.
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("48376829-2728-4918-93C4-3A12955F57D6")]
public partial interface IImageFormatProviderExtension : IFormatProviderExtension
{
    /// <summary>
    /// Format the novel into this format.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="imageStream">Original image stream</param>
    /// <param name="destinationStream">Destination formatted image stream</param>
    void FormatImage(ITaskCompletionSource task, IStream imageStream, IStream destinationStream);
}

public static class ImageFormatProviderExtensionHelper
{
    /// <inheritdoc cref="IImageFormatProviderExtension.FormatImage"/>
    public static async Task FormatImageAsync(this IImageFormatProviderExtension extension, IStream imageStream, IStream destinationStream)
    {
        var wrapper = new TaskCompletionSourceWrapper(new());
        extension.FormatImage(wrapper, imageStream, destinationStream);
        await wrapper.Task;
        if (extension.GetFormatExceptionMessage() is { } result)
            ThrowHelper.Format(result);
    }
}

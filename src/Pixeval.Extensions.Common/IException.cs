using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("AFB6EE92-4DD4-4E51-A32C-27921BF83950")]
public partial interface IException
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetTypeName();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetMessage();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string? GetSource();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string? GetStackTrace();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string? GetHelpLink();

    [EditorBrowsable(EditorBrowsableState.Never)]
    IException? GetInnerException();
}

public static partial class ExtensionHelper
{
    extension(IException iException)
    {
        /// <inheritdoc cref="IException.GetTypeName"/>
        public string TypeName => iException.GetTypeName();

        /// <inheritdoc cref="IException.GetMessage"/>
        public string Message => iException.GetMessage();

        /// <inheritdoc cref="IException.GetSource"/>
        public string? Source => iException.GetSource();

        /// <inheritdoc cref="IException.GetStackTrace"/>
        public string? StackTrace => iException.GetStackTrace();

        /// <inheritdoc cref="IException.GetHelpLink"/>
        public string? HelpLink => iException.GetHelpLink();

        /// <inheritdoc cref="IException.GetInnerException"/>
        public IException? InnerException => iException.GetInnerException();
    }
}

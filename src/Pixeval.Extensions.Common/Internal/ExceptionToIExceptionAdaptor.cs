    // Copyright (c) Pixeval.Extensions.Common
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// Adapts a <see cref="Exception"/> to implement <see cref="IException"/> interface for COM interop.
/// </summary>
[GeneratedComClass]
internal sealed partial class ExceptionToIExceptionAdaptor : Exception, IException
{
    private readonly string _typeName;
    private readonly IException? _innerException;

    public ExceptionToIExceptionAdaptor(Exception exception) : base(exception.Message)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _typeName = exception.GetType().FullName ?? exception.GetType().Name;
        Source = exception.Source;
        StackTrace = exception.StackTrace;
        HelpLink = exception.HelpLink;
        _innerException = exception.InnerException?.ToIException();
    }

    /// <inheritdoc />
    public override string? StackTrace { get; }

    /// <inheritdoc />
    string IException.GetTypeName() => _typeName;

    /// <inheritdoc />
    string IException.GetMessage() => Message;

    /// <inheritdoc />
    string? IException.GetSource() => Source;

    /// <inheritdoc />
    string? IException.GetStackTrace() => StackTrace;

    /// <inheritdoc />
    string? IException.GetHelpLink() => HelpLink;

    /// <inheritdoc />
    IException? IException.GetInnerException() => _innerException;
}

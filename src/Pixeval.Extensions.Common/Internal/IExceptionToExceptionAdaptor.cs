// Copyright (c) Pixeval.Extensions.Common
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// Adapts an <see cref="IException"/> to behave as a <see cref="Exception"/> for managed code.
/// Wraps the COM exception information in a managed exception structure.
/// </summary>
internal sealed class IExceptionToExceptionAdaptor : Exception
{
    public IExceptionToExceptionAdaptor(IException iException)
        : base(iException.Message, iException.InnerException?.ToException())
    {
        ArgumentNullException.ThrowIfNull(iException);
        Source = iException.Source;
        HelpLink = iException.HelpLink;
        TypeName = iException.TypeName;
        StackTrace = iException.StackTrace;
    }

    /// <inheritdoc />
    public override string? StackTrace { get; }

    public string TypeName { get; }
}

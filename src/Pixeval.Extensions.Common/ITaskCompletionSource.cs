// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("CAB05B3A-321C-43DE-8A21-B2819999E97F")]
public partial interface ITaskCompletionSource
{
    void SetCompleted();

    [EditorBrowsable(EditorBrowsableState.Never)]
    void SetException(IException exception);
}

public static partial class ExtensionHelper
{
    extension(ITaskCompletionSource source)
    {
        public void SetException(Exception exception) => source.SetException(exception.ToIException());
    }
}

// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("CAB05B3A-321C-43DE-8A21-B2819999E97F")]
public partial interface ITaskCompletionSource
{
    void SetCompleted();

    void SetException(string message);
}

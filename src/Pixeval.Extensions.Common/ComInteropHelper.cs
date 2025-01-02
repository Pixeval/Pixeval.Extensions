// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Pixeval.Extensions.Common.Internal;

namespace Pixeval.Extensions.Common;

public static class ComInteropHelper
{
    public static IStream ToIStream(this Stream stream) => new Internal.NetToComStream(stream);

    public static Stream ToStream(this IStream iStream) => new ComToNetStream(iStream);

    public static unsafe STATSTG GetStat(this IStream ioStream)
    {
        var stat = new STATSTG();
        ioStream.Stat((nint)Unsafe.AsPointer(ref stat), 0);
        return stat;
    }

    public static ITaskCompletionSource ToITaskCompletionSource(this TaskCompletionSource taskCompletionSource) => new Internal.TaskCompletionSourceWrapper(taskCompletionSource);
}

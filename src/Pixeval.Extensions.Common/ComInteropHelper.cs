// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.IO;
using System.Threading.Tasks;
using Pixeval.Extensions.Common.Internal;

namespace Pixeval.Extensions.Common;

public static class ComInteropHelper
{
    public static IStream ToIStream(this Stream stream) => new StreamAdaptor(stream);

    public static Stream ToStream(this IStream iStream) => new StreamAdaptor(iStream);

    public static int Read(this IStream iStream, byte[] buffer) => iStream.Read(buffer, 0, buffer.Length);

    public static void Write(this IStream iStream, byte[] buffer) => iStream.Write(buffer, 0, buffer.Length);

    public static async Task<int> ReadAsync(this IStream iStream, byte[] buffer, int offset, int count)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.ReadAsync(taskCompletionSource.ToITaskCompletionSource(), buffer, offset, count);
        await taskCompletionSource.Task;
        return iStream.GetReadAsyncResult();
    }

    public static async Task WriteAsync(this IStream iStream, byte[] buffer, int offset, int count)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.WriteAsync(taskCompletionSource.ToITaskCompletionSource(), buffer, offset, count);
        await taskCompletionSource.Task;
    }

    public static async Task<int> ReadAsync(this IStream iStream, byte[] buffer)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.ReadAsync(taskCompletionSource.ToITaskCompletionSource(), buffer, 0, buffer.Length);
        await taskCompletionSource.Task;
        return iStream.GetReadAsyncResult();
    }

    public static async Task WriteAsync(this IStream iStream, byte[] buffer)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.WriteAsync(taskCompletionSource.ToITaskCompletionSource(), buffer, 0, buffer.Length);
        await taskCompletionSource.Task;
    }

    public static async Task FlushAsync(this IStream iStream)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.FlushAsync(taskCompletionSource.ToITaskCompletionSource());
        await taskCompletionSource.Task;
    }

    public static async Task CopyToAsync(this IStream iStream, IStream destination, int bufferSize = -1)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.CopyToAsync(taskCompletionSource.ToITaskCompletionSource(), destination, bufferSize);
        await taskCompletionSource.Task;
    }

    public static async Task DisposeAsync(this IStream iStream)
    {
        var taskCompletionSource = new TaskCompletionSource();
        iStream.DisposeAsync(taskCompletionSource.ToITaskCompletionSource());
        await taskCompletionSource.Task;
    }

    public static ITaskCompletionSource ToITaskCompletionSource(this TaskCompletionSource taskCompletionSource) => new TaskCompletionSourceWrapper(taskCompletionSource);
}

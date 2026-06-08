// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common;

public static class ExtensionHelper
{
    extension(IStream iStream)
    {
        public int Read(byte[] buffer) => iStream.Read(buffer, 0, bufferCount: buffer.Length);

        public void Write(byte[] buffer) => iStream.Write(buffer, 0, bufferCount: buffer.Length);

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            var source = new TaskCompletionSource();
            iStream.ReadAsync(source.ToITaskCompletionSource(), buffer, offset, bufferCount: count);
            await source.Task;
            return iStream.GetReadResult();
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            var source = new TaskCompletionSource();
            iStream.WriteAsync(source.ToITaskCompletionSource(), buffer, offset, bufferCount: count);
            await source.Task;
        }

        public async Task<int> ReadAsync(byte[] buffer)
        {
            var source = new TaskCompletionSource();
            iStream.ReadAsync(source.ToITaskCompletionSource(), buffer, 0, bufferCount: buffer.Length);
            await source.Task;
            return iStream.GetReadResult();
        }

        public async Task WriteAsync(byte[] buffer)
        {
            var source = new TaskCompletionSource();
            iStream.WriteAsync(source.ToITaskCompletionSource(), buffer, 0, bufferCount: buffer.Length);
            await source.Task;
        }

        public async Task FlushAsync()
        {
            var source = new TaskCompletionSource();
            iStream.FlushAsync(source.ToITaskCompletionSource());
            await source.Task;
        }

        public async Task CopyToAsync(IStream destination, int bufferSize = -1)
        {
            var source = new TaskCompletionSource();
            iStream.CopyToAsync(source.ToITaskCompletionSource(), destination, bufferSize);
            await source.Task;
        }

        public async Task DisposeAsync()
        {
            var source = new TaskCompletionSource();
            iStream.DisposeAsync(source.ToITaskCompletionSource());
            await source.Task;
        }
    }

    extension(ITaskCompletionSource source)
    {
        public void SetException(Exception exception) => source.SetException(exception.ToIException());
    }

    extension(IProgressNotifier notifier)
    {
        public void Aborted(Exception exception) => notifier.Aborted(exception.ToIException());
    }
}

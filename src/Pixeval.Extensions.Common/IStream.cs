// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface]
[Guid("F243EBCC-C42A-4294-AD28-A2C6D5579A62")]
public partial interface IStream
{
    /// <inheritdoc cref="Stream.CanSeek" />
    [return: MarshalAs(UnmanagedType.Bool)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool GetCanSeek();

    /// <inheritdoc cref="Stream.CanRead" />
    [return: MarshalAs(UnmanagedType.Bool)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool GetCanRead();

    /// <inheritdoc cref="Stream.CanWrite" />
    [return: MarshalAs(UnmanagedType.Bool)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool GetCanWrite();

    /// <inheritdoc cref="Stream.Position" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    long GetPosition();

    /// <inheritdoc cref="Stream.Position" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void SetPosition(long value);

    /// <inheritdoc cref="Stream.Length" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    long GetLength();

    /// <inheritdoc cref="Stream.SetLength" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void SetLength(long value);

    /// <inheritdoc cref="Stream.Seek"/>
    long Seek(long offset, SeekOrigin origin);

    /// <inheritdoc cref="Stream.Read(byte[], int, int)" />
    int Read([MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void ReadAsync(ITaskCompletionSource task, [MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetReadAsyncResult();

    /// <inheritdoc cref="Stream.Write(byte[], int, int)" />
    void Write([MarshalUsing(CountElementName = nameof(bufferSize))] byte[] buffer, int offset, int bufferSize);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void WriteAsync(ITaskCompletionSource task, [MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.Flush" />
    void Flush();

    /// <inheritdoc cref="Stream.FlushAsync()" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void FlushAsync(ITaskCompletionSource task);

    /// <inheritdoc cref="Stream.CopyTo(Stream, int)" />
    void CopyTo(IStream destination, int bufferSize = -1);

    /// <inheritdoc cref="Stream.CopyToAsync(Stream, int)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void CopyToAsync(ITaskCompletionSource task, IStream destination, int bufferSize = -1);

    #region IDisposable

    /// <inheritdoc cref="IDisposable.Dispose" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void Dispose();

    #endregion

    #region IAsyncDisposable

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    void DisposeAsync(ITaskCompletionSource task);

    #endregion
}

public static partial class ExtensionHelper
{
    extension(IStream iStream)
    {
        /// <inheritdoc cref="IStream.GetCanSeek"/>
        public bool CanSeek => iStream.GetCanSeek();

        /// <inheritdoc cref="IStream.GetCanRead"/>
        public bool CanRead => iStream.GetCanRead();

        /// <inheritdoc cref="IStream.GetCanWrite"/>
        public bool CanWrite => iStream.GetCanWrite();

        /// <inheritdoc cref="IStream.GetPosition"/>
        /// <inheritdoc cref="IStream.SetPosition"/>
        public long Position
        {
            get => iStream.GetPosition();
            set => iStream.SetPosition(value);
        }

        /// <inheritdoc cref="IStream.GetLength"/>
        public long Length => iStream.GetLength();

        /// <inheritdoc cref="IStream.Read"/>
        public int Read(byte[] buffer) => iStream.Read(buffer, 0, buffer.Length);

        /// <inheritdoc cref="IStream.Write"/>
        public void Write(byte[] buffer) => iStream.Write(buffer, 0, buffer.Length);

        /// <inheritdoc cref="IStream.ReadAsync"/>
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            var source = new TaskCompletionSource();
            iStream.ReadAsync(source.ToITaskCompletionSource(), buffer, offset, count);
            await source.Task;
            return iStream.GetReadAsyncResult();
        }

        /// <inheritdoc cref="IStream.WriteAsync"/>
        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            var source = new TaskCompletionSource();
            iStream.WriteAsync(source.ToITaskCompletionSource(), buffer, offset, count);
            await source.Task;
        }

        /// <inheritdoc cref="IStream.ReadAsync"/>
        public async Task<int> ReadAsync(byte[] buffer)
        {
            var source = new TaskCompletionSource();
            iStream.ReadAsync(source.ToITaskCompletionSource(), buffer, 0, buffer.Length);
            await source.Task;
            return iStream.GetReadAsyncResult();
        }

        /// <inheritdoc cref="IStream.WriteAsync"/>
        public async Task WriteAsync(byte[] buffer)
        {
            var source = new TaskCompletionSource();
            iStream.WriteAsync(source.ToITaskCompletionSource(), buffer, 0, buffer.Length);
            await source.Task;
        }

        /// <inheritdoc cref="IStream.FlushAsync"/>
        public async Task FlushAsync()
        {
            var source = new TaskCompletionSource();
            iStream.FlushAsync(source.ToITaskCompletionSource());
            await source.Task;
        }

        /// <inheritdoc cref="IStream.CopyToAsync"/>
        public async Task CopyToAsync(IStream destination, int bufferSize = -1)
        {
            var source = new TaskCompletionSource();
            iStream.CopyToAsync(source.ToITaskCompletionSource(), destination, bufferSize);
            await source.Task;
        }

        /// <inheritdoc cref="IStream.DisposeAsync"/>
        public async Task DisposeAsync()
        {
            var source = new TaskCompletionSource();
            iStream.DisposeAsync(source.ToITaskCompletionSource());
            await source.Task;
        }
    }
}

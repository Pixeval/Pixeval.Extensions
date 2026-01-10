// Copyright (c) Pixeval.Extensions.Common
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// Adapts a <see cref="Stream"/> to implement <see cref="IStream"/> interface for COM interop.
/// </summary>
[GeneratedComClass]
internal partial class StreamToIStreamAdaptor : Stream, IStream
{
    private readonly Stream _ioStream;
    private int _readAsyncResult;

    public StreamToIStreamAdaptor(Stream ioStream)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        _ioStream = ioStream;
    }

    /// <inheritdoc />
    public override bool CanSeek => _ioStream.CanSeek;

    /// <inheritdoc />
    public override bool CanRead => _ioStream.CanRead;

    /// <inheritdoc />
    public override bool CanWrite => _ioStream.CanWrite;

    /// <inheritdoc />
    public override long Position
    {
        get => _ioStream.Position;
        set => _ioStream.Position = value;
    }

    /// <inheritdoc />
    public override long Length => _ioStream.Length;

    /// <inheritdoc />
    bool IStream.GetCanSeek() => CanSeek;

    /// <inheritdoc />
    bool IStream.GetCanRead() => CanRead;

    /// <inheritdoc />
    bool IStream.GetCanWrite() => CanWrite;

    /// <inheritdoc />
    long IStream.GetPosition() => Position;

    /// <inheritdoc />
    void IStream.SetPosition(long value) => Position = value;

    /// <inheritdoc />
    long IStream.GetLength() => Length;

    /// <inheritdoc cref="Stream.SetLength" />
    public override void SetLength(long value) => _ioStream.SetLength(value);

    /// <inheritdoc cref="Stream.Seek" />
    public override long Seek(long offset, SeekOrigin origin) => _ioStream.Seek(offset, origin);

    public override int Read(byte[] buffer, int offset, int count) => _ioStream.Read(buffer, offset, count);

    /// <inheritdoc />
    void IStream.ReadAsync(ITaskCompletionSource task, byte[] buffer, int offset, int count) =>
        SimpleAwaiter(task, async () => _readAsyncResult = await _ioStream.ReadAsync(buffer.AsMemory(offset, count)));

    /// <inheritdoc />
    int IStream.GetReadAsyncResult() => _readAsyncResult;

    public override void Write(byte[] buffer, int offset, int count) => _ioStream.Write(buffer, offset, count);

    public void WriteAsync(ITaskCompletionSource task, byte[] buffer, int offset, int count) =>
        SimpleAwaiter(task, async () => await _ioStream.WriteAsync(buffer.AsMemory(offset, count)));

    /// <inheritdoc cref="Stream.Flush" />
    public override void Flush() => _ioStream.Flush();

    /// <inheritdoc />
    void IStream.FlushAsync(ITaskCompletionSource task) => SimpleAwaiter(task, _ioStream.FlushAsync);

    /// <inheritdoc />
    public void CopyTo(IStream destination, int bufferSize = -1)
    {
        if (bufferSize < 0)
            _ioStream.CopyTo(destination.ToStream());
        else
            _ioStream.CopyTo(destination.ToStream(), bufferSize);
    }

    /// <inheritdoc />
    void IStream.CopyToAsync(ITaskCompletionSource task, IStream destination, int bufferSize) =>
        SimpleAwaiter(task, async () =>
        {
            if (bufferSize < 0)
                await _ioStream.CopyToAsync(destination.ToStream());
            else
                await _ioStream.CopyToAsync(destination.ToStream(), bufferSize);
        });

    /// <inheritdoc />
    void IStream.DisposeAsync(ITaskCompletionSource task) =>
        SimpleAwaiter(task, async () => await _ioStream.DisposeAsync());

    public override void Close()
    {
        _ioStream.Dispose();
#pragma warning disable CA1816
        GC.SuppressFinalize(this);
#pragma warning restore CA1816
    }

    public override async ValueTask DisposeAsync()
    {
        await _ioStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private static async void SimpleAwaiter(ITaskCompletionSource source, Func<Task> task)
    {
        try
        {
            await task();
            source.SetCompleted();
        }
        catch (Exception e)
        {
            source.SetException(e.ToIException());
        }
    }
}

// Copyright (c) Pixeval.Extensions.Common
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// The class ManagedIStream is not COM-visible. Its purpose is to be able to invoke COM interfaces
/// from managed code rather than the contrary.
/// </summary>
[GeneratedComClass]
internal partial class StreamAdaptor : Stream, IStream
{
    public StreamAdaptor(Stream ioStream)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        _ioStream = ioStream;
        IStreamIsNull = true;
    }

    public StreamAdaptor(IStream iStream)
    {
        ArgumentNullException.ThrowIfNull(iStream);
        _iStream = iStream;
        IStreamIsNull = false;
    }

    /// <inheritdoc />
    public override bool CanSeek => IStreamIsNull ? _ioStream.CanSeek : _iStream.GetCanSeek();

    /// <inheritdoc />
    public override bool CanRead => IStreamIsNull ? _ioStream.CanRead : _iStream.GetCanRead();

    /// <inheritdoc />
    public override bool CanWrite => IStreamIsNull ? _ioStream.CanWrite : _iStream.GetCanWrite();

    /// <inheritdoc />
    public override long Position
    {
        get => IStreamIsNull ? _ioStream.Position : _iStream.GetPosition();
        set
        {
            if (IStreamIsNull)
                _ioStream.Position = value;
            else
                _iStream.SetPosition(value);
        }
    }

    /// <inheritdoc />
    public override long Length => IStreamIsNull ? _ioStream.Length : _iStream.GetLength();

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
    public override void SetLength(long value)
    {
        if (IStreamIsNull)
            _ioStream.SetLength(value);
        else
            _iStream.SetLength(value);
    }

    /// <inheritdoc cref="Stream.Seek" />
    public override long Seek(long offset, SeekOrigin origin) => IStreamIsNull ? _ioStream.Seek(offset, origin) : _iStream.Seek(offset, origin);

    public override int Read(byte[] buffer, int offset, int count) => IStreamIsNull ? _ioStream.Read(buffer, offset, count) : _iStream.Read(buffer, offset, count);

    private int _readAsyncResult;

    /// <inheritdoc />
    void IStream.ReadAsync(ITaskCompletionSource task, byte[] buffer, int offset, int count)
    {
        if (IStreamIsNull)
            SimpleAwaiter(task, async () => _readAsyncResult = await _ioStream.ReadAsync(buffer.AsMemory(offset, count)));
        else
            _iStream.ReadAsync(task, buffer, offset, count);
    }

    /// <inheritdoc />
    int IStream.GetReadAsyncResult() => IStreamIsNull ? _readAsyncResult : _iStream.GetReadAsyncResult();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (IStreamIsNull)
            _ioStream.Write(buffer, offset, count);
        else
            _iStream.Write(buffer, offset, count);
    }

    public void WriteAsync(ITaskCompletionSource task, byte[] buffer, int offset, int count)
    {
        if (IStreamIsNull)
            SimpleAwaiter(task, async () => await _ioStream.WriteAsync(buffer.AsMemory(offset, count)));
        else
            _iStream.WriteAsync(task, buffer, offset, count);
    }

    /// <inheritdoc cref="Stream.Flush" />
    public override void Flush()
    {
        if (IStreamIsNull)
            _ioStream.Flush();
        else
            _iStream.Flush();
    }

    /// <inheritdoc />
    void IStream.FlushAsync(ITaskCompletionSource task)
    {
        if (IStreamIsNull)
            SimpleAwaiter(task, _ioStream.FlushAsync);
        else
            _iStream.FlushAsync(task);
    }

    /// <inheritdoc />
    public void CopyTo(IStream destination, int bufferSize = -1)
    {
        if (IStreamIsNull)
            if (bufferSize < 0)
                _ioStream.CopyTo(destination.ToStream());
            else
                _ioStream.CopyTo(destination.ToStream(), bufferSize);
        else
            _iStream.CopyTo(destination, bufferSize);
    }

    /// <inheritdoc />
    void IStream.CopyToAsync(ITaskCompletionSource task, IStream destination, int bufferSize)
    {

        if (IStreamIsNull)
            SimpleAwaiter(task, async () =>
            {
                if (bufferSize < 0)
                    await _ioStream.CopyToAsync(destination.ToStream());
                else
                    await _ioStream.CopyToAsync(destination.ToStream(), bufferSize);
            });
        else
            _iStream.CopyToAsync(task, destination, bufferSize);
    }

    /// <inheritdoc />
    void IStream.DisposeAsync(ITaskCompletionSource task)
    {
        if (IStreamIsNull)
            SimpleAwaiter(task, async () => await _ioStream.DisposeAsync());
        else
            _iStream.DisposeAsync(task);
    }

    public override void Close()
    {
        if (IStreamIsNull)
            _ioStream.Dispose();
        else
            _iStream.Dispose();
#pragma warning disable CA1816
        GC.SuppressFinalize(this);
#pragma warning restore CA1816
    }

    public override async ValueTask DisposeAsync()
    {
        if (IStreamIsNull)
            await _ioStream.DisposeAsync();
        else
            await _iStream.ToStream().DisposeAsync();
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
            source.SetException(e.Message);
        }
    }

    [MemberNotNullWhen(true, nameof(_ioStream))]
    [MemberNotNullWhen(false, nameof(_iStream))]
    private bool IStreamIsNull { get; }

    private readonly Stream? _ioStream;

    private readonly IStream? _iStream;
}

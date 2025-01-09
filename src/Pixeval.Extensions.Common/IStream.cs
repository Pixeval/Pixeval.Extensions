// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface]
[Guid("F243EBCC-C42A-4294-AD28-A2C6D5579A62")]
public partial interface IStream
{
    /// <inheritdoc cref="Stream.CanSeek" />
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetCanSeek();

    /// <inheritdoc cref="Stream.CanRead" />
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetCanRead();

    /// <inheritdoc cref="Stream.CanWrite" />
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetCanWrite();

    /// <inheritdoc cref="Stream.Position" />
    long GetPosition();

    /// <inheritdoc cref="Stream.Position" />
    void SetPosition(long value);

    /// <inheritdoc cref="Stream.Length" />
    long GetLength();

    /// <inheritdoc cref="Stream.SetLength" />
    void SetLength(long value);

    /// <inheritdoc cref="Stream.Seek"/>
    long Seek(long offset, SeekOrigin origin);

    /// <inheritdoc cref="Stream.Read(byte[], int, int)" />
    int Read([MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int)" />
    void ReadAsync(ITaskCompletionSource task, [MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int)" />
    int GetReadAsyncResult();

    /// <inheritdoc cref="Stream.Write(byte[], int, int)" />
    void Write([MarshalUsing(CountElementName = nameof(bufferSize))] byte[] buffer, int offset, int bufferSize);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int)" />
    void WriteAsync(ITaskCompletionSource task, [MarshalUsing(CountElementName = nameof(count))] byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.Flush" />
    void Flush();

    /// <inheritdoc cref="Stream.FlushAsync()" />
    void FlushAsync(ITaskCompletionSource task);

    /// <inheritdoc cref="Stream.CopyTo(Stream, int)" />
    void CopyTo(IStream destination, int bufferSize = -1);

    /// <inheritdoc cref="Stream.CopyToAsync(Stream, int)" />
    void CopyToAsync(ITaskCompletionSource task, IStream destination, int bufferSize = -1);

    #region IDisposable

    /// <inheritdoc cref="IDisposable.Dispose" />
    void Dispose();

    #endregion

    #region IAsyncDisposable

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync" />
    void DisposeAsync(ITaskCompletionSource task);

    #endregion
}

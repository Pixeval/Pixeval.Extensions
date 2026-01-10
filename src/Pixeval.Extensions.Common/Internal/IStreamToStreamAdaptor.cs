// Copyright (c) Pixeval.Extensions.Common
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// Adapts an <see cref="IStream"/> to behave as a <see cref="Stream"/> for managed code.
/// </summary>
internal class IStreamToStreamAdaptor : Stream
{
    private readonly IStream _iStream;

    public IStreamToStreamAdaptor(IStream iStream)
    {
        ArgumentNullException.ThrowIfNull(iStream);
        _iStream = iStream;
    }

    /// <inheritdoc />
    public override bool CanSeek => _iStream.CanSeek;

    /// <inheritdoc />
    public override bool CanRead => _iStream.CanRead;

    /// <inheritdoc />
    public override bool CanWrite => _iStream.CanWrite;

    /// <inheritdoc />
    public override long Position
    {
        get => _iStream.Position;
        set => _iStream.Position = value;
    }

    /// <inheritdoc />
    public override long Length => _iStream.Length;

    /// <inheritdoc cref="Stream.SetLength" />
    public override void SetLength(long value) => _iStream.SetLength(value);

    /// <inheritdoc cref="Stream.Seek" />
    public override long Seek(long offset, SeekOrigin origin) => _iStream.Seek(offset, origin);

    public override int Read(byte[] buffer, int offset, int count) => _iStream.Read(buffer, offset, count);

    public override void Write(byte[] buffer, int offset, int count) => _iStream.Write(buffer, offset, count);

    /// <inheritdoc cref="Stream.Flush" />
    public override void Flush() => _iStream.Flush();

    public override void Close()
    {
        _iStream.Dispose();
#pragma warning disable CA1816
        GC.SuppressFinalize(this);
#pragma warning restore CA1816
    }

    public override async ValueTask DisposeAsync()
    {
        await _iStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.IO;

namespace Pixeval.Extensions.Common;

internal class ComToNetStream : Stream
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ComToNetStream(IStream iStream)
    {
        ArgumentNullException.ThrowIfNull(iStream);
        _iStream = iStream;
    }

    public override void Flush() => _iStream.Commit(0 /*STGC_DEFAULT*/);

    public override int Read(byte[] buffer, int offset, int count)
    {
        _iStream.Read(buffer, (uint)count, out var bytesRead);
        return (int)bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _iStream.Seek(offset, origin, out var newPosition);
        return newPosition;
    }

    public override void SetLength(long value) => _iStream.SetSize(value);

    public override void Write(byte[] buffer, int offset, int count) => _iStream.Write(buffer, count, out _);

    public override bool CanSeek => true;

    public override bool CanRead => (_iStream.GetStat().grfMode & 2) is not (int)NetToComStream.STGM_ACCESS.STGM_WRITE;

    public override bool CanWrite => (_iStream.GetStat().grfMode & 2) is not (int)NetToComStream.STGM_ACCESS.STGM_READ;

    public override long Length => _iStream.GetStat().cbSize;

    public override long Position
    {
        get => Seek(0, SeekOrigin.Current);
        set => Seek(value, SeekOrigin.Begin);
    }

    private readonly IStream _iStream;
}

// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

/// <summary>
/// The class ManagedIStream is not COM-visible. Its purpose is to be able to invoke COM interfaces
/// from managed code rather than the contrary.
/// </summary>
[GeneratedComClass]
[Guid("49BE742F-D551-48A6-A32F-6A05E85EB2CD")]
internal partial class NetToComStream : IStream, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NetToComStream(Stream ioStream)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        _ioStream = ioStream;
    }

    /// <summary>
    /// Move the stream pointer to the specified position.
    /// </summary>
    /// <remarks>
    /// System.IO.stream supports searching past the end of the stream, like
    /// OLE streams.
    /// newPositionPtr is not an out parameter because the method is required
    /// to accept NULL pointers.
    /// </remarks>
    void IStream.Seek(long offset, SeekOrigin origin, out long newPosition)
    {
        // The operation will generally be I/O bound, so there is no point in
        // eliminating the following switch by playing on the fact that
        // System.IO uses the same integer values as IStream for SeekOrigin.
        // Dereference newPositionPtr and assign to the pointed location.
        newPosition = _ioStream.Seek(offset, origin);
    }

    /// <summary>
    /// Read at most bufferSize bytes into buffer and return the effective
    /// number of bytes read in bytesReadPtr (unless null).
    /// </summary>
    /// <remarks>
    /// mscorlib disassembly shows the following MarshalAs parameters
    /// void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pv, int cb, nint pcbRead);
    /// This means marshaling code will have found the size of the array buffer in the parameter bufferSize.
    /// </remarks>
    void IStream.Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, uint bufferSize, out long bytesRead)
    {
        bytesRead = _ioStream.Read(buffer, 0, (int)bufferSize);
    }

    /// <summary>
    /// Sets stream's size.
    /// </summary>
    void IStream.SetSize(long libNewSize) => _ioStream.SetLength(libNewSize);

    /// <summary>
    /// Write at most bufferSize bytes from buffer.
    /// </summary>
    void IStream.Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int bufferSize, out long bytesWritten)
    {
        _ioStream.Write(buffer, 0, bufferSize);
        // If fewer than bufferSize bytes had been written, an exception would
        // have been thrown, so it can be assumed we wrote bufferSize bytes.
        bytesWritten = bufferSize;
    }

    unsafe void IStream.Stat(nint pstatstg, int grfStatFlag)
    {
        ref var streamStats = ref Unsafe.AsRef<STATSTG>((void*)pstatstg);
        streamStats.type = (int)STGTY.STGTY_STREAM;
        streamStats.cbSize = _ioStream.Length;

        if (_ioStream is { CanRead: true, CanWrite: true })
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_READWRITE;
        else if (_ioStream.CanRead)
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_READ;
        else if (_ioStream.CanWrite)
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_WRITE;
        else
        {
            // A stream that is neither readable nor writable is a closed stream.
            // Note the use of an exception that is known to the interop marshaller
            // (unlike ObjectDisposedException).
            throw new IOException();
        }
    }

    [Flags]
    public enum STGM_ACCESS
    {
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,

        STGM_SHARE_DENY_NONE = 0x00000040,
        STGM_SHARE_DENY_READ = 0x00000030,
        STGM_SHARE_DENY_WRITE = 0x00000020,
        STGM_SHARE_EXCLUSIVE = 0x00000010,
        STGM_PRIORITY = 0x00040000,

        STGM_CREATE = 0x00001000,
        STGM_CONVERT = 0x00020000,
        STGM_FAILIFTHERE = 0x00000000,

        STGM_DIRECT = 0x00000000,
        STGM_TRANSACTED = 0x00010000,

        STGM_NOSCRATCH = 0x00100000,
        STGM_NOSNAPSHOT = 0x00200000,

        STGM_SIMPLE = 0x08000000,
        STGM_DIRECT_SWMR = 0x00400000,

        STGM_DELETEONRELEASE = 0x04000000
    }

    public enum STGTY
    {
        STGTY_STORAGE = 1,
        STGTY_STREAM = 2,
        STGTY_LOCKBYTES = 3,
        STGTY_PROPERTY = 4
    }

    /// <summary>
    /// Read at most bufferSize bytes from the receiver and write them to targetStream.
    /// </summary>
    /// <remarks>
    /// Not implemented.
    /// </remarks>
    void IStream.CopyTo(IStream targetStream, long bufferSize, out long totalBytesRead, out long totalBytesWritten)
    {
        var sourceBytes = new byte[bufferSize];
        totalBytesRead = 0;
        totalBytesWritten = 0;

        while (totalBytesWritten < bufferSize)
        {
            var currentBytesRead = _ioStream.Read(sourceBytes, 0, (int)(bufferSize - totalBytesWritten));

            // Has the end of the stream been reached?
            if (currentBytesRead is 0)
                break;

            totalBytesRead += currentBytesRead;

            targetStream.Write(sourceBytes, currentBytesRead, out var currentBytesWritten);
            if (currentBytesWritten != currentBytesRead)
            {
                System.Diagnostics.Debug.WriteLine("ERROR!: The IStream Write is not writing all the bytes needed!");
            }

            totalBytesWritten += currentBytesWritten;
        }
    }

    /// <summary>
    /// Commit changes.
    /// </summary>
    void IStream.Commit(uint flags) => _ioStream.Flush();

    /// <summary>Closes the current stream and releases any resources (such as the Stream) associated with the current IStream.</summary>
    /// <returns></returns>
    /// <remarks>This method is not a member in IStream.</remarks>
    public void Dispose()
    {
        _ioStream?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _ioStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    #region Unimplemented methods

    /// <summary>
    /// Create a clone.
    /// </summary>
    /// <remarks>
    /// Not implemented.
    /// </remarks>
    void IStream.Clone(out IStream streamCopy)
    {
        streamCopy = null!;
        throw new NotSupportedException();
    }

    /// <summary>
    /// Lock at most byteCount bytes starting at offset.
    /// </summary>
    /// <remarks>
    /// Not supported by System.IO.Stream.
    /// </remarks>
    void IStream.LockRegion(long offset, long byteCount, uint lockType)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Undo writes performed since last Commit.
    /// </summary>
    /// <remarks>
    /// Relevant only to transacted streams.
    /// </remarks>
    void IStream.Revert()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Unlock the specified region.
    /// </summary>
    /// <remarks>
    /// Not supported by System.IO.Stream.
    /// </remarks>
    void IStream.UnlockRegion(long offset, long byteCount, uint lockType)
    {
        throw new NotSupportedException();
    }

    #endregion Unimplemented methods

    private readonly Stream _ioStream;
}

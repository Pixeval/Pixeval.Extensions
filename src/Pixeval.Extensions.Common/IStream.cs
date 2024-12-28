// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface]
[Guid("0000000c-0000-0000-C000-000000000046")]
public partial interface IStream
{
    void Seek(long offset, SeekOrigin origin, out long newPosition);

    void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, uint bufferSize, out long bytesRead);

    void Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int bufferSize, out long bytesWritten);

    void SetSize(long libNewSize);

    void CopyTo(IStream targetStream, long bufferSize, out long buffer, out int bytesWritten);

    void Commit(uint flags);

    void Revert();

    void LockRegion(long offset, long byteCount, uint lockType);

    void UnlockRegion(long offset, long byteCount, uint lockType);

    void Stat(nint pstatstg, int grfStatFlag);

    void Clone(out IStream ppstm);
}

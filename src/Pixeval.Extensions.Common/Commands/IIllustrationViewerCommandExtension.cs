// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Commands;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("A3A3B685-BB4B-4FFE-8970-0844DEC590FA")]
public partial interface IIllustrationViewerCommandExtension : IViewerCommandExtension
{
    IExtension GetSubExtension();
}

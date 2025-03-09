// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Downloaders;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("848517E4-6CA0-40CE-9F8E-3C0E2108284A")]
public partial interface IDownloaderExtension : IExtension
{
    void Download(IProgressNotifier notifier, string uri, string destination);
}

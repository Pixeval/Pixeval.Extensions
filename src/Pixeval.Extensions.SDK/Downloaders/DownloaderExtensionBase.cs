// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Downloaders;

namespace Pixeval.Extensions.SDK.Downloaders;

/// <inheritdoc cref="IDownloaderExtension" />
[GeneratedComClass]
public abstract partial class DownloaderExtensionBase : ExtensionBase, IDownloaderExtension
{
    /// <inheritdoc />
    public abstract void Download(IProgressNotifier notifier, string uri, string destination);
}

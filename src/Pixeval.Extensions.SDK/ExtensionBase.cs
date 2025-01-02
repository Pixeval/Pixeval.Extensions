// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions.SDK;

[GeneratedComClass]
public abstract partial class ExtensionBase : IExtension
{
    /// <inheritdoc />
    public abstract void OnExtensionLoaded();

    /// <inheritdoc />
    public abstract void OnExtensionUnloaded();
}

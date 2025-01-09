#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/ExtensionsMetadataBase.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions.SDK;

/// <inheritdoc />
[GeneratedComClass]
public abstract partial class ExtensionsHostBase : IExtensionsHost
{
    /// <inheritdoc cref="IExtensionsHost.GetExtensionName" />
    public abstract string ExtensionName { get; }

    /// <inheritdoc cref="IExtensionsHost.GetAuthorName" />
    public abstract string AuthorName { get; }

    /// <inheritdoc cref="IExtensionsHost.GetExtensionLink" />
    public abstract string ExtensionLink { get; }

    /// <inheritdoc cref="IExtensionsHost.GetHelpLink" />
    public abstract string HelpLink { get; }

    /// <inheritdoc cref="IExtensionsHost.GetDescription" />
    public abstract string Description { get; }

    /// <inheritdoc cref="IExtensionsHost.GetVersion" />
    public abstract string Version { get; }

    /// <inheritdoc cref="IExtensionsHost.GetExtensions" />
    public abstract IExtension[] Extensions { get; }

    /// <inheritdoc cref="IExtensionsHost.GetIcon" />
    public abstract byte[]? Icon { get; }

    /// <inheritdoc />
    public abstract void Initialize(string cultureName, string tempDirectory, string extensionDirectory);

    public static unsafe int DllGetExtensionsHost(void** ppv, ExtensionsHostBase current)
    {
        if (_CcwCache is null)
        {
            var comWrappers = new StrategyBasedComWrappers();
            _CcwCache = (void*)comWrappers.GetOrCreateComInterfaceForObject(current, CreateComInterfaceFlags.None);
        }

        *ppv = _CcwCache;
        return 0;
    }

    private static unsafe void* _CcwCache;

    /// <inheritdoc />
    string IExtensionsHost.GetExtensionName() => ExtensionName;

    /// <inheritdoc />
    string IExtensionsHost.GetAuthorName() => AuthorName;

    /// <inheritdoc />
    string IExtensionsHost.GetExtensionLink() => ExtensionLink;

    /// <inheritdoc />
    string IExtensionsHost.GetHelpLink() => HelpLink;

    /// <inheritdoc />
    string IExtensionsHost.GetDescription() => Description;

    /// <inheritdoc />
    string IExtensionsHost.GetVersion() => Version;

    /// <inheritdoc />
    int IExtensionsHost.GetExtensionsCount() => Extensions.Length;

    /// <inheritdoc />
    IExtension[] IExtensionsHost.GetExtensions(int count) => count == Extensions.Length ? Extensions : [];

    /// <inheritdoc />
    int IExtensionsHost.GetIconBytesCount() => Icon?.Length ?? -1;

    /// <inheritdoc />
    byte[]? IExtensionsHost.GetIcon(int count) => count == Icon?.Length ? Icon : null;
}

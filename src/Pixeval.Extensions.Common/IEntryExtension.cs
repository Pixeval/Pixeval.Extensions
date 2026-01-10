// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("9FBAAD2C-6B0E-4288-B1CF-7E0CAE0E44AB")]
public partial interface IEntryExtension : IExtension
{
    /// <summary>
    /// 图标
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    Symbol GetIcon();

    /// <summary>
    /// 标题
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetLabel();

    /// <summary>
    /// 描述
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetDescription();
}

public static partial class ExtensionHelper
{
    extension(IEntryExtension extension)
    {
        /// <inheritdoc cref="IEntryExtension.GetIcon"/>
        public Symbol Icon => extension.GetIcon();

        /// <inheritdoc cref="IEntryExtension.GetLabel"/>
        public string Label => extension.GetLabel();

        /// <inheritdoc cref="IEntryExtension.GetDescription"/>
        public string Description => extension.GetDescription();
    }
}

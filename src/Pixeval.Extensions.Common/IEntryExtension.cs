// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("9FBAAD2C-6B0E-4288-B1CF-7E0CAE0E44AB")]
public partial interface IEntryExtension : IExtension
{
    /// <summary>
    /// 设置项图标
    /// </summary>
    Symbol GetIcon();

    /// <summary>
    /// 标题
    /// </summary>
    string GetLabel();

    /// <summary>
    /// 描述
    /// </summary>
    string GetDescription();
}

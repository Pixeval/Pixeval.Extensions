#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/IntSettingsExtension.cs
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
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("E19AF543-8F41-491B-B37C-DF1130434260")]
public abstract partial class IntSettingsExtensionBase : SettingsExtensionBase, IIntSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Int;

    /// <inheritdoc cref="IIntSettingsExtension.GetDefaultValue" />
    public abstract int DefaultValue { get; }

    /// <inheritdoc cref="IIntSettingsExtension.GetMinValue" />
    public abstract int MinValue { get; }

    /// <inheritdoc cref="IIntSettingsExtension.GetMaxValue" />
    public abstract int MaxValue { get; }

    /// <inheritdoc cref="IIntSettingsExtension.GetLargeChange" />
    public virtual int LargeChange => 10;

    /// <inheritdoc cref="IIntSettingsExtension.GetSmallChange" />
    public virtual int SmallChange => 1;

    /// <inheritdoc cref="IIntSettingsExtension.GetPlaceholder" />
    public abstract string? Placeholder { get; }

    /// <inheritdoc />
    int IIntOrEnumSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    int IIntSettingsExtension.GetMinValue() => MinValue;
    
    /// <inheritdoc />
    int IIntSettingsExtension.GetMaxValue() => MaxValue;

    /// <inheritdoc />
    int IIntSettingsExtension.GetLargeChange() => LargeChange;

    /// <inheritdoc />
    int IIntSettingsExtension.GetSmallChange() => SmallChange;

    /// <inheritdoc />
    string? IIntSettingsExtension.GetPlaceholder() => Placeholder;
}

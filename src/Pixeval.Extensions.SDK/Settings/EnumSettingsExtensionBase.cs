#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/EnumSettingsExtensionBase.cs
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

using System;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Extensions.SDK.Internal;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IEnumSettingsExtension" />
[GeneratedComClass]
public abstract partial class EnumSettingsExtensionBase : SettingsExtensionBase, IEnumSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Enum;

    /// <inheritdoc cref="IEnumSettingsExtension.GetDefaultValue" />
    public abstract int DefaultValue { get; }

    public abstract string[]? EnumStrings { get; }

    public abstract Type EnumType { get; }

    /// <inheritdoc cref="IEnumSettingsExtension.OnValueChanged" />
    public abstract void OnValueChanged(int value);

    /// <inheritdoc />
    int IIntOrEnumSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    void IEnumSettingsExtension.GetEnumKeyValues(out int count, out string[] enumNames, out int[] enumValues)
    {
        enumValues = (int[])EnumType.GetEnumValuesAsUnderlyingType().GetArray(out count);
        enumNames = EnumType.GetEnumNames().GetArray(out count);
    }
}

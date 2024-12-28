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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("4BBC875E-FB75-44C6-935F-D9B09E7044DB")]
public abstract partial class EnumSettingsExtensionBase : SettingsExtensionBase, IEnumSettingsExtension
{
    public override SettingsType SettingsType => SettingsType.Enum;

    public abstract int DefaultValue { get; }

    public abstract Type EnumType { get; }

    int IEnumSettingsExtension.GetDefaultValue() => DefaultValue;

    int IEnumSettingsExtension.GetEnumCount() => EnumType.GetEnumValuesAsUnderlyingType().Length;

    void IEnumSettingsExtension.GetEnumKeyValues(int count, out string[] enumNames, out int[] enumValues)
    {
        var values = EnumType.GetEnumValuesAsUnderlyingType();
        enumValues = count != values.Length ? [] : (int[])values;
        var names = EnumType.GetEnumNames();
        enumNames = count != names.Length ? [] : names;
    }
}

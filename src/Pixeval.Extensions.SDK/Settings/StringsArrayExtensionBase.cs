#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/StringsArrayExtensionBase.cs
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
[Guid("29E75ED9-784A-4F5D-95A8-F7187569435E")]
public abstract partial class StringsArrayExtensionBase : SettingsExtensionBase, IStringsArrayExtension
{
    public override SettingsType SettingsType => SettingsType.StringsArray;

    public abstract string[] DefaultValue { get; }

    int IStringsArrayExtension.GetDefaultValueCount() => DefaultValue.Length;

    string[] IStringsArrayExtension.GetDefaultValue(int count) => count == DefaultValue.Length ? DefaultValue : [];
}

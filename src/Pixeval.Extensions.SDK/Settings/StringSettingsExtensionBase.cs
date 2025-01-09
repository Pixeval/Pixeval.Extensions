#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/StringSettingsExtensionBase.cs
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

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IStringSettingsExtension" />
[GeneratedComClass]
public abstract partial class StringSettingsExtensionBase : SettingsExtensionBase, IStringSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.String;

    /// <inheritdoc cref="GetPlaceholder" />
    public abstract string? Placeholder { get; }

    /// <inheritdoc />
    public abstract string GetDefaultValue();

    /// <inheritdoc />
    public string? GetPlaceholder() => Placeholder;

    /// <inheritdoc />
    public abstract void OnValueChanged(string value);
}

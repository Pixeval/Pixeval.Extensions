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

[Guid("5027FB37-9BEB-480D-8725-0E6F34B494E2")]
[GeneratedComClass]
public abstract partial class ExtensionsHostBase : IExtensionsHost
{
    public abstract string ExtensionName { get; }

    public abstract IExtension[] Extensions { get; }

    /// <inheritdoc />
    public string GetExtensionName() => ExtensionName;

    /// <inheritdoc />
    int IExtensionsHost.GetExtensionsCount() => Extensions.Length;

    /// <inheritdoc />
    IExtension[] IExtensionsHost.GetExtension(int count) => count == Extensions.Length ? Extensions : [];

    public abstract void Initialize(string cultureBcl47);

    public abstract void OnStringPropertyChanged(string name, string value);

    public abstract void OnIntOrEnumPropertyChanged(string name, int value);

    public abstract void OnDoublePropertyChanged(string name, double value);

    public abstract void OnColorPropertyChanged(string name, uint value);

    public abstract void OnBoolPropertyChanged(string name, bool value);

    public void OnStringsArrayPropertyChanged(string name, string[] value, int count)
    {
        if (count == value.Length)
            OnStringsArrayPropertyChanged(name, value);
    }

    public abstract void OnStringsArrayPropertyChanged(string name, string[] value);
}

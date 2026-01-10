// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("71EA31D8-437F-4B0C-80A4-8FC6DBC2AB65")]
public partial interface IIntSettingsExtension : IIntOrEnumSettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetMinValue();

    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetMaxValue();

    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetStepValue();
}

public static partial class SettingsExtensionHelper
{
    extension(IIntSettingsExtension extension)
    {
        /// <inheritdoc cref="IIntSettingsExtension.GetMinValue"/>
        public int MinValue => extension.GetMinValue();

        /// <inheritdoc cref="IIntSettingsExtension.GetMaxValue"/>
        public int MaxValue => extension.GetMaxValue();

        /// <inheritdoc cref="IIntSettingsExtension.GetStepValue"/>
        public int StepValue => extension.GetStepValue();
    }
}

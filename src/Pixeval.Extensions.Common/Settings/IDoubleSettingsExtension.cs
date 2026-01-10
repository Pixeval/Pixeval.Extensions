// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("06583FB2-4577-4E2A-B7F1-654B7C504D01")]
public partial interface IDoubleSettingsExtension : ISettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    double GetDefaultValue();

    [EditorBrowsable(EditorBrowsableState.Never)]
    double GetMinValue();

    [EditorBrowsable(EditorBrowsableState.Never)]
    double GetMaxValue();

    [EditorBrowsable(EditorBrowsableState.Never)]
    double GetStepValue();

    void OnValueChanged(double value);
}

public static partial class SettingsExtensionHelper
{
    extension(IDoubleSettingsExtension extension)
    {
        /// <inheritdoc cref="IDoubleSettingsExtension.GetDefaultValue"/>
        public double DefaultValue => extension.GetDefaultValue();

        /// <inheritdoc cref="IDoubleSettingsExtension.GetMinValue"/>
        public double MinValue => extension.GetMinValue();

        /// <inheritdoc cref="IDoubleSettingsExtension.GetMaxValue"/>
        public double MaxValue => extension.GetMaxValue();

        /// <inheritdoc cref="IDoubleSettingsExtension.GetStepValue"/>
        public double StepValue => extension.GetStepValue(); 
    }
}

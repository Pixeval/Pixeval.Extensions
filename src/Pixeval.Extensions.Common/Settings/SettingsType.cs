// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

namespace Pixeval.Extensions.Common.Settings;

public enum SettingsType
{
    /// <summary>
    /// Actual type is <see cref="bool"/>
    /// </summary>
    Bool,

    /// <summary>
    /// Actual type is <see cref="int"/>
    /// </summary>
    Int,

    /// <summary>
    /// Actual type is <see cref="double"/>
    /// </summary>
    Double,

    /// <summary>
    /// Actual type is <see cref="string"/>
    /// </summary>
    String,

    /// <summary>
    /// Actual type is <see cref="string"/>[]
    /// </summary>
    StringsArray,

    /// <summary>
    /// Actual type is <see cref="int"/>
    /// </summary>
    Enum,

    /// <summary>
    /// Actual type is <see cref="uint"/>
    /// </summary>
    Color,

    /// <summary>
    /// Actual type is <see cref="string"/>
    /// </summary>
    Font,

    /// <summary>
    /// Actual type is <see cref="int"/> and <see cref="long"/>
    /// </summary>
    DateTimeOffset
}

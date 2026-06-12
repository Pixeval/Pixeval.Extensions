using System.Collections.Generic;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class PidlModel
{
    public List<EnumDefinition> Enums { get; } = [];

    public List<InterfaceDefinition> Interfaces { get; } = [];

    public required PidlMetadata Metadata { get; set; }

    public string Version => Metadata.Version;

    public string SdkVersion
    {
        get
        {
            var version = System.Version.Parse(Version);
            return string.Join(
                ".",
                version.Major,
                version.Minor,
                version.Build < 0 ? 0 : version.Build,
                version.Revision < 0 ? 0 : version.Revision);
        }
    }
}

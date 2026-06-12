using System.Runtime.InteropServices;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class PidlMetadata
{
    public required string Version { get; init; }

    public required StringMarshalling StringMarshalling { get; init; }

    public required UnmanagedType BoolMarshalling { get; init; }
}
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Pixeval.Extensions.Generator;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Pixeval.Extensions.Generator <common|sdk> [metadata.json pidl...] <output>");
            return 1;
        }

        var target = args[0];
        if (!target.Equals("common", StringComparison.OrdinalIgnoreCase) &&
            !target.Equals("sdk", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Target must be 'common' or 'sdk'.");
            return 1;
        }

        if (args.Length < 4)
        {
            Console.Error.WriteLine("PIDL targets require <metadata.json> <pidl...> <output>.");
            return 1;
        }

        var inputs = args.Skip(1).Take(args.Length - 2).ToArray();
        var output = args[^1];
        foreach (var input in inputs)
        {
            if (!File.Exists(input))
            {
                Console.Error.WriteLine($"IDL file was not found: {input}");
                return 1;
            }
        }

        var generated = PixevalExtensionsCodeGenerator.Generate(target, inputs.Select(File.ReadAllText));
        var outputDirectory = Path.GetDirectoryName(output);
        if (!string.IsNullOrEmpty(outputDirectory))
            _ = Directory.CreateDirectory(outputDirectory);

        File.WriteAllText(output, generated, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return 0;
    }
}

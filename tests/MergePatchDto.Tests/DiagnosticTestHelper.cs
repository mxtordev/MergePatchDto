using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MergePatch;
using MergePatchDto.Generators;

namespace MergePatchDto.Tests;

internal static class DiagnosticTestHelper
{
    public static IReadOnlyList<Diagnostic> GetDiagnostics(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var compilation = CSharpCompilation.Create(
            "MergePatchDtoDiagnosticsTests",
            [syntaxTree],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver.Create(
            generators: [new MergePatchGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        return generatorDiagnostics
            .Concat(outputCompilation.GetDiagnostics())
            .Where(diagnostic => diagnostic.Id.StartsWith("MPD", StringComparison.Ordinal))
            .ToArray();
    }

    private static IReadOnlyList<MetadataReference> GetReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (trustedPlatformAssemblies == null)
        {
            return
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MergePatchAttribute).Assembly.Location)
            ];
        }

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(MergePatchAttribute).Assembly.Location))
            .ToArray();
    }
}

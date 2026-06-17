using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MergePatch;
using MergePatchDto.Generators;

namespace MergePatchDto.Tests;

internal static class DiagnosticTestHelper
{
    public static IReadOnlyList<Diagnostic> GetDiagnostics(
        string source,
        IEnumerable<MetadataReference>? additionalReferences = null)
    {
        return GetAllDiagnostics(source, additionalReferences)
            .Where(diagnostic => diagnostic.Id.StartsWith("MPD", StringComparison.Ordinal))
            .ToArray();
    }

    public static IReadOnlyList<Diagnostic> GetAllDiagnostics(
        string source,
        IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var references = additionalReferences == null
            ? GetReferences()
            : GetReferences().Concat(additionalReferences).ToArray();

        var compilation = CSharpCompilation.Create(
            "MergePatchDtoDiagnosticsTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver.Create(
            generators: [new MergePatchGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        return generatorDiagnostics
            .Concat(outputCompilation.GetDiagnostics())
            .ToArray();
    }

    public static MetadataReference CreateReference(string assemblyName, string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, emitResult.Diagnostics);
            throw new InvalidOperationException("Failed to create metadata reference:" + Environment.NewLine + diagnostics);
        }

        stream.Position = 0;
        return MetadataReference.CreateFromStream(stream);
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

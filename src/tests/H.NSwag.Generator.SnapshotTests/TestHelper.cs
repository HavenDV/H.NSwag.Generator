using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace H.Generators.IntegrationTests;

public static class TestHelper
{
    public static async Task CheckSourceAsync(
        this VerifyBase verifier,
        AdditionalText[] additionalTexts,
        CancellationToken cancellationToken = default)
    {
        var dotNetFolder = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
        var compilation = (Compilation)CSharpCompilation.Create(
            assemblyName: "Tests",
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Private.Uri.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Net.Http.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.ObjectModel.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Collections.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Net.Primitives.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Runtime.Serialization.Primitives.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetFolder, "System.Text.Json.dll")),
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.KeyAttribute).Assembly.Location),
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new NSwagGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create(additionalTexts))
            .RunGeneratorsAndUpdateCompilation(compilation, out compilation, out _, cancellationToken);
        var diagnostics = compilation.GetDiagnostics(cancellationToken);

        await Task.WhenAll(
            verifier
                .Verify(diagnostics)
                .UseDirectory("Snapshots")
                .UseTextForParameters("Diagnostics"),
            verifier
                .Verify(driver)
                .UseDirectory("Snapshots"));
    }
}
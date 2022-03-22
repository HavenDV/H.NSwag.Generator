using System.Collections.Immutable;
using H.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace H.Generators.IntegrationTests;

public static class TestHelper
{
    public static async Task CheckSource(
        this VerifyBase verifier,
        params AdditionalText[] texts)
    {
        var dotNetFolder = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
        var compilation = (Compilation)CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
"),
            },
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
            });
        var generator = new NSwagGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create(texts));
        driver = driver.RunGenerators(compilation);
        
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out compilation,
            out _);
        var diagnostics = compilation.GetDiagnostics();

        await verifier
            .Verify(diagnostics)
            .UseDirectory("Snapshots")
            .UseTextForParameters("Diagnostics");
        await verifier
            .Verify(driver)
            .UseDirectory("Snapshots");
    }
}
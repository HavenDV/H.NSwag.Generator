using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.NSwag.Generator.IntegrationTests
{
    [TestClass]
    public class NSwagGeneratorTests
    {
        private static string ConsolePath
        {
            get
            {
                var value = "%USERPROFILE%/.nuget/packages/nswag.msbuild/13.11.3/tools/Net50/dotnet-nswag.dll";
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    value = value.Replace("%USERPROFILE%", "/home/runner");
                }

                return value;
            }
        }

        [TestMethod]
        public void GenerateTest()
        {
            var text = Resources.openapi1;
            var path = Path.GetTempFileName();
            File.WriteAllText(path, text);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            }

            var source = NSwagGenerator.Generate(
                ConsolePath,
                path);

            Console.WriteLine(source);
        }

        [TestMethod]
        public void GenerateFailedTest()
        {
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var text = Resources.openapi2;
                var path = Path.GetTempFileName();
                File.WriteAllText(path, text);

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                }

                var _ = NSwagGenerator.Generate(
                    ConsolePath,
                    path);
            });

            Console.WriteLine(exception);
        }

        [TestMethod]
        public void ExecuteTest()
        {
            var text = Resources.openapi1;
            var path = Path.GetTempFileName() + ".nswag";
            File.WriteAllText(path, text);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            }

            var inputCompilation = CSharpCompilation.Create(
                "compilation",
                new[] { CSharpSyntaxTree.ParseText(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
") },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var generator = new NSwagGenerator();
            var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(
                new ISourceGenerator[] { generator },
                new AdditionalText[] { new CustomAdditionalText(path) }, 
                CSharpParseOptions.Default,
                new CustomAnalyzerConfigOptionsProvider());

            driver.RunGeneratorsAndUpdateCompilation(
                inputCompilation, 
                out var outputCompilation, 
                out var diagnostics);

            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(2);

            var source = outputCompilation.SyntaxTrees.ElementAt(1).GetText().ToString();
            Console.WriteLine(source);
        }

        public class CustomAdditionalText : AdditionalText
        {
            public string Text { get; }

            public override string Path { get; }

            public CustomAdditionalText(string path)
            {
                Path = path;
                Text = File.ReadAllText(path);
            }

            public override SourceText GetText(CancellationToken cancellationToken = default)
            {
                return SourceText.From(Text);
            }
        }

        public class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value)
            {
                value = ConsolePath;

                return true;
            }
        }

        public class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            public override AnalyzerConfigOptions GlobalOptions { get; } = new CustomAnalyzerConfigOptions();

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            {
                throw new NotImplementedException();
            }

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            {
                throw new NotImplementedException();
            }
        }
    }
}

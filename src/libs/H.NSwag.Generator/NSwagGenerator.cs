using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace H.NSwag.Generator
{
    [Generator]
    public class NSwagGenerator : ISourceGenerator
    {
        #region Methods

        public void Execute(GeneratorExecutionContext context)
        {
            var file = context.AdditionalFiles
                           .FirstOrDefault(file =>
                               file.Path.EndsWith(".nswag", StringComparison.InvariantCultureIgnoreCase))
                       ?? throw new InvalidOperationException(".nswag file is not found.");

            var consolePath = GetGlobalOption(
                context,
                "NSwagConsolePath",
                GetGlobalOption(context, "NSwagExe_Net50", @"C:\Program Files (x86)\Rico Suter\NSwagStudio\Net50\dotnet-nswag.exe"));

            var code = NSwagGeneratorCore.Generate(consolePath, file.Path);

            context.AddSource("NSwag Generated CSharp Code", SourceText.From(code, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        #endregion

        #region Utilities

        private static string GetGlobalOption(GeneratorExecutionContext context, string name, string? defaultValue = null)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var result) &&
                   !string.IsNullOrWhiteSpace(result)
                ? result
                : defaultValue ?? string.Empty;
        }

        #endregion
    }
}
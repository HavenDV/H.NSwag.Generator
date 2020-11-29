using System;
using System.Linq;
using System.Text;
using H.NSwag.Generator.Core.Extensions;
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

            var runtime = file.GetText()?.ToString().ExtractAll("\"runtime\": \"", "\"").First();
            var defaultDirectoryOptionName = runtime switch
            {
                "WinX64" or "WinX86" => "NSwagDir",
                "NetCore21" => "NSwagDir_Core21",
                "NetCore22" => "NSwagDir_Core22",
                "NetCore30" => "NSwagDir_Core30",
                "NetCore31" => "NSwagDir_Core31",
                "Net50" => "NSwagDir_Net50",
                _ => throw new InvalidOperationException($"Invalid runtime: {runtime}"),
            };
            var programFilesSubDir = runtime switch
            {
                "WinX64" or "WinX86" => "Win",
                _ => runtime,
            };
            var exeName = runtime switch
            {
                "WinX86" => "NSwag.x86.exe",
                "WinX64" => "NSwag.exe",
                _ => "dotnet-nswag.exe",
            };

            var defaultConsolePath = GetGlobalOption(context, defaultDirectoryOptionName);
            defaultConsolePath = string.IsNullOrWhiteSpace(defaultConsolePath)
                ? @$"C:\Program Files (x86)\Rico Suter\NSwagStudio\{programFilesSubDir}\{exeName}"
                : $"{defaultConsolePath}{exeName}";

            var consolePath = GetGlobalOption(context, "NSwagConsolePath", defaultConsolePath);

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
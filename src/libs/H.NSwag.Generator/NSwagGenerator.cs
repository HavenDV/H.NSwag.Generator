using System;
using System.Diagnostics;
using System.IO;
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
            try
            {
                var file = context.AdditionalFiles
                               .FirstOrDefault(text =>
                                   text.Path.EndsWith(".nswag", StringComparison.InvariantCultureIgnoreCase))
                           ?? throw new InvalidOperationException(".nswag file is not found.");

                var runtime = file.GetText()?.ToString().ExtractAll("\"runtime\": \"", "\"").First();
                var defaultDirectoryOptionName = runtime switch
                {
                    "WinX64" or "WinX86" => "NSwagDir",
                    "NetCore21" => "NSwagDir_Core21",
                    "NetCore22" => "NSwagDir_Core22",
                    "NetCore30" => "NSwagDir_Core30",
                    "NetCore31" => "NSwagDir_Core31",
                    "Net50" or "Default" => "NSwagDir_Net50",
                    _ => throw new InvalidOperationException($"Invalid runtime: {runtime}"),
                };
                var fileName = runtime switch
                {
                    "WinX86" => "NSwag.x86.exe",
                    "WinX64" => "NSwag.exe",
                    _ => "dotnet-nswag.dll",
                };

                var defaultDirectory = GetGlobalOption(context, defaultDirectoryOptionName);
                if (string.IsNullOrWhiteSpace(defaultDirectory))
                {
                    throw new InvalidOperationException(
                        $"Default directory global option({defaultDirectoryOptionName}) is null or empty.");
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                var defaultConsolePath = Path.Combine(defaultDirectory, fileName);

                var consolePath = GetGlobalOption(context, "NSwagConsolePath") ?? defaultConsolePath;

                var code = Generate(consolePath, file.Path);

                context.AddSource("NSwag Generated CSharp Code", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "NSG0001",
                            "Exception: ",
                            $"{exception}",
                            "Usage",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public static string Generate(string consolePath, string nswagPath)
        {
            consolePath = consolePath ?? throw new ArgumentNullException(nameof(consolePath));
            nswagPath = nswagPath ?? throw new ArgumentNullException(nameof(nswagPath));

            var nswagTempDir = $"{Path.GetTempFileName().Replace(".tmp", string.Empty)}";
            Directory.CreateDirectory(nswagTempDir);
            Directory.SetCurrentDirectory(nswagTempDir);

            var nswagTempPath = Path.Combine(nswagTempDir, "temp.nswag");
            var outputPath = $"{Path.GetTempFileName()}.cs".Replace('\\', '/');

            try
            {
                File.Copy(nswagPath, nswagTempPath, true);

                var nswagContents = File.ReadAllText(nswagTempPath);

                nswagContents = nswagContents.Replace("\"output\": null,", "\"output\": \"\",");
                var (start, length) = nswagContents.ExtractAllIndexes("\"output\": \"", "\"").Last();
                nswagContents = nswagContents
                    .Remove(start, length)
                    .Insert(start, outputPath);

                File.WriteAllText(nswagTempPath, nswagContents);

                var isDll = consolePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                var fileName = isDll
                    ? "dotnet"
                    : Environment.ExpandEnvironmentVariables(consolePath);
                var arguments = isDll
                    ? $"\"{Environment.ExpandEnvironmentVariables(consolePath)}\" run"
                    : "run";
                using var process = Process.Start(new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });

                process?.WaitForExit();
                
                var output = process?.StandardOutput.ReadToEnd().Replace('\n', ' ');
                var error = process?.StandardError.ReadToEnd().Replace('\n', ' ');

                try
                {
                    return File.ReadAllText(outputPath);
                }
                catch (FileNotFoundException exception)
                {
                    throw new InvalidOperationException($"NSwag console error. Output: {output}. Error: {error}", exception);
                }
            }
            finally
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                if (File.Exists(nswagTempPath))
                {
                    File.Delete(nswagTempPath);
                }
            }
        }

        #endregion

        #region Utilities

        private static string? GetGlobalOption(GeneratorExecutionContext context, string name)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var result) &&
                   !string.IsNullOrWhiteSpace(result)
                ? result
                : null;
        }

        #endregion
    }
}
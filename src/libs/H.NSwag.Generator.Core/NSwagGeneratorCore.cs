using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using H.NSwag.Generator.Core.Extensions;

namespace H.NSwag.Generator
{
    public static class NSwagGeneratorCore
    {
        public static string Generate(string consolePath, string nswagPath)
        {
            consolePath = consolePath ?? throw new ArgumentNullException(nameof(consolePath));
            nswagPath = nswagPath ?? throw new ArgumentNullException(nameof(nswagPath));

            var nswagTempPath = $"{Path.GetTempFileName()}.nswag";
            var outputPath = $"{Path.GetTempFileName()}.cs";

            try
            {
                File.Copy(nswagPath, nswagTempPath, true);

                var nswagContents = File.ReadAllText(nswagTempPath);
                var outputIndex = nswagContents.ExtractAllIndexes("\"output\": \"", "\"").Last();
                nswagContents = nswagContents
                    .Remove(outputIndex.Start, outputIndex.Length)
                    .Insert(outputIndex.Start, outputPath.Replace('\\', '/'));

                File.WriteAllText(nswagTempPath, nswagContents);

                using var process = Process.Start(new ProcessStartInfo(
                    Environment.ExpandEnvironmentVariables(consolePath),
                    $"run \"{nswagTempPath}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                });

                process?.WaitForExit();

                return File.ReadAllText(outputPath);
            }
            finally
            {
                File.Delete(outputPath);
                File.Delete(nswagTempPath);
            }
        }
    }
}
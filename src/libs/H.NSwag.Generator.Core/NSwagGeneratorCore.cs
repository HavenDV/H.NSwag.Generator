using System;
using System.Diagnostics;
using System.IO;

namespace H.NSwag.Generator
{
    public static class NSwagGeneratorCore
    {
        public static string Generate(string consolePath, string nswagPath, string generatedPath)
        {
            consolePath = consolePath ?? throw new ArgumentNullException(nameof(consolePath));
            nswagPath = nswagPath ?? throw new ArgumentNullException(nameof(nswagPath));
            generatedPath = generatedPath ?? throw new ArgumentNullException(nameof(generatedPath));

            using var process = Process.Start(new ProcessStartInfo(
                Environment.ExpandEnvironmentVariables(consolePath),
                $"run \"{nswagPath}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            });

            process?.WaitForExit();

            return File.ReadAllText(Environment.ExpandEnvironmentVariables(generatedPath));
        }
    }
}
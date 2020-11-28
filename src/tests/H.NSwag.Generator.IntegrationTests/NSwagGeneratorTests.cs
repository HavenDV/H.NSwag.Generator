using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using H.NSwag.Generator.IntegrationTests.Utilities;

namespace H.NSwag.Generator.IntegrationTests.IntegrationTests
{
    [TestClass]
    public class NSwagGeneratorTests
    {
        [TestMethod]
        public void GenerateTest()
        {
            var text = ResourcesUtilities.ReadFileAsString("openapi.nswag");
            var path = Path.GetTempFileName();
            File.WriteAllText(path, text);

            var source = NSwagGeneratorCore.Generate(
                @"%USERPROFILE%/.nuget/packages/nswag.msbuild/13.9.4/tools/Net50/dotnet-nswag.exe",
                path, 
                @"%TEMP%/H.NSwag.Generator/Generated.cs");

            Console.WriteLine(source);
        }
    }
}

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using H.NSwag.Generator.IntegrationTests.Utilities;

namespace H.NSwag.Generator.IntegrationTests.IntegrationTests
{
    [TestClass]
    public class NSwagGeneratorTests
    {
        [DataTestMethod]
        [DataRow("openapi1.nswag")]
        public void GenerateTest(string name)
        {
            var text = ResourcesUtilities.ReadFileAsString(name);
            var path = Path.GetTempFileName();
            File.WriteAllText(path, text);

            var source = NSwagGeneratorCore.Generate(
                @"%USERPROFILE%/.nuget/packages/nswag.msbuild/13.9.4/tools/Net50/dotnet-nswag.exe",
                path);

            Console.WriteLine(source);
        }

        [DataTestMethod]
        [DataRow("openapi2.nswag")]
        public void GenerateFailedTest(string name)
        {
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var text = ResourcesUtilities.ReadFileAsString(name);
                var path = Path.GetTempFileName();
                File.WriteAllText(path, text);

                var source = NSwagGeneratorCore.Generate(
                    @"%USERPROFILE%/.nuget/packages/nswag.msbuild/13.9.4/tools/Net50/dotnet-nswag.exe",
                    path);
            });

            Console.WriteLine(exception);
        }
    }
}

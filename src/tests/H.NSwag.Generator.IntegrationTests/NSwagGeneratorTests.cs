using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.NSwag.Generator.IntegrationTests
{
    [TestClass]
    public class NSwagGeneratorTests
    {
        [TestMethod]
        public void GenerateTest()
        {
            var text = H.Resources.openapi1;
            var path = Path.GetTempFileName();
            File.WriteAllText(path, text);

            var source = NSwagGenerator.Generate(
                @"%USERPROFILE%/.nuget/packages/nswag.msbuild/13.11.3/tools/Net50/dotnet-nswag.exe",
                path);

            Console.WriteLine(source);
        }

        [TestMethod]
        public void GenerateFailedTest()
        {
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var text = H.Resources.openapi2;
                var path = Path.GetTempFileName();
                File.WriteAllText(path, text);

                var source = NSwagGenerator.Generate(
                    @"%USERPROFILE%/.nuget/packages/nswag.msbuild/13.11.3/tools/Net50/dotnet-nswag.exe",
                    path);
            });

            Console.WriteLine(exception);
        }
    }
}

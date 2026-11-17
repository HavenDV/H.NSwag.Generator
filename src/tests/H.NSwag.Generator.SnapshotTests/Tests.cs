using System.Net;
using System.Reflection;
using H.Generators.Tests.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json.Linq;

namespace H.Generators.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public Task Empty()
    {
        return this.CheckSourceAsync(
            Array.Empty<AdditionalText>());
    }

    [TestMethod]
    public Task InsideYaml()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(InsideYaml)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);

        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }

    [TestMethod]
    public Task YamlWithLocalFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(YamlWithLocalFile)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);

        File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "openapi.yaml"), Resources.openapi_yaml.AsBytes());

        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }

    [TestMethod]
    public Task YamlWithUrl()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(YamlWithUrl)}.nswag");
        var text = Resources.openapi_from_url_nswag.AsString();
        File.WriteAllText(path, text);

        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }

    [TestMethod]
    public async Task ResponseBodyReadsHonorCancellation()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nswag");
        var document = JObject.Parse(Resources.openapi_from_yaml_nswag.AsString());
        document["documentGenerator"]!["fromDocument"] = new JObject
        {
            ["json"] = """
                {
                  "openapi": "3.0.0",
                  "info": { "title": "Cancellation test", "version": "1.0.0" },
                  "servers": [{ "url": "https://example.invalid" }],
                  "paths": {
                    "/probe": {
                      "get": {
                        "operationId": "Probe",
                        "responses": { "200": { "description": "OK" } }
                      }
                    }
                  }
                }
                """,
        };
        var settings = document["codeGenerators"]!["openApiToCSharpClient"]!;
        settings["className"] = "CancellationClient";
        settings["namespace"] = "CancellationTests";

        try
        {
            await File.WriteAllTextAsync(path, document.ToString());
            var source = await NSwagGenerator.GenerateAsync(path);
            var referenceAssemblies = LatestReferenceAssemblies.Net80
                .WithPackages([new PackageIdentity("Newtonsoft.Json", "13.0.1")]);
            var references = await referenceAssemblies.ResolveAsync(null, CancellationToken.None);
            var compilation = CSharpCompilation.Create(
                assemblyName: $"CancellationTests_{Guid.NewGuid():N}",
                syntaxTrees: [CSharpSyntaxTree.ParseText(source,
                    new CSharpParseOptions(preprocessorSymbols: ["NET5_0_OR_GREATER"]))],
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var assemblyStream = new MemoryStream();
            var result = compilation.Emit(assemblyStream);
            result.Success.Should().BeTrue(string.Join(Environment.NewLine,
                result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

            var assembly = Assembly.Load(assemblyStream.ToArray());
            var clientType = assembly.GetType("CancellationTests.CancellationClient", throwOnError: true)!;
            using var content = new BlockingResponseContent();
            using var httpClient = new HttpClient(new ErrorResponseHandler(content));
            using var cancellation = new CancellationTokenSource();
            var client = Activator.CreateInstance(clientType, httpClient);
            var method = clientType.GetMethod("ProbeAsync", [typeof(CancellationToken)])!;
            var request = (Task)method.Invoke(client, [cancellation.Token])!;
            try
            {
                await content.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                cancellation.Cancel();
                await Task.WhenAny(request, Task.Delay(TimeSpan.FromSeconds(5)));
                request.IsCanceled.Should().BeTrue(
                    "caller cancellation must interrupt reads of an untyped error response body");
            }
            finally
            {
                content.Release.TrySetResult();
                await request.ContinueWith(static task => task.Exception, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class ErrorResponseHandler : HttpMessageHandler
    {
        private readonly HttpContent _content;

        public ErrorResponseHandler(HttpContent content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = _content,
            });
        }
    }

    private sealed class BlockingResponseContent : HttpContent
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return SerializeToStreamAsync(stream, context, CancellationToken.None);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            return Release.Task.WaitAsync(cancellationToken);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
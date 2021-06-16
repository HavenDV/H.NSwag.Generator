using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace H.NSwag.Generator
{
    [Generator]
    public class NSwagGenerator : ISourceGenerator
    {
        #region Methods

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var text in context.AdditionalFiles
                .Where(static text => text.Path.EndsWith(
                    ".nswag", 
                    StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    context.AddSource(
                        $"{Path.GetFileName(text.Path)}.cs", 
                        SourceText.From(Generate(text.Path), Encoding.UTF8));
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
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public static string Generate(string path)
        {
            path = path ?? throw new ArgumentNullException(nameof(path));

            var json = File.ReadAllText(path);
            var document = 
                JsonConvert.DeserializeObject<NSwagDocument>(json) ??
                throw new InvalidOperationException("Document is null.");
            var settings = document.CodeGenerators.OpenApiToCSharpClient;
            var fromDocument = document.DocumentGenerator.FromDocument;

            var openApi = Task.Run(() => 
                string.IsNullOrWhiteSpace(fromDocument.Url)
                    ? fromDocument.Json.StartsWith("{", StringComparison.OrdinalIgnoreCase)
                        ? OpenApiDocument.FromJsonAsync(fromDocument.Json)
                        : OpenApiYamlDocument.FromYamlAsync(fromDocument.Json)
                    : OpenApiDocument.FromUrlAsync(fromDocument.Url))
                .Result;
            var generator = new CSharpClientGenerator(openApi, new CSharpClientGeneratorSettings
            {
                ClassName = settings.ClassName,
                AdditionalContractNamespaceUsages = settings.AdditionalContractNamespaceUsages,
                AdditionalNamespaceUsages = settings.AdditionalNamespaceUsages,
                ChecksumCacheEnabled = settings.ChecksumCacheEnabled,
                ClientBaseClass = settings.ClientBaseClass,
                ClientBaseInterface = settings.ClientBaseInterface,
                ClientClassAccessModifier = settings.ClientClassAccessModifier,
                ConfigurationClass = settings.ConfigurationClass,
                DisposeHttpClient = settings.DisposeHttpClient,
                ExceptionClass = settings.ExceptionClass,
                ProtectedMethods = settings.ProtectedMethods,
                CSharpGeneratorSettings =
                {
                    Namespace = settings.Namespace,
                    GenerateNullableReferenceTypes = settings.GenerateNullableReferenceTypes,
                    AnyType = settings.AnyType,
                    ArrayBaseType = settings.ArrayBaseType,
                    ArrayInstanceType = settings.ArrayInstanceType,
                    ArrayType = settings.ArrayType,
                    ClassStyle = settings.ClassStyle,
                    DateTimeType = settings.DateTimeType,
                    DateType = settings.DateType,
                    DictionaryBaseType = settings.DictionaryBaseType,
                },
                GenerateOptionalParameters = settings.GenerateOptionalParameters,
                GenerateBaseUrlProperty = settings.GenerateBaseUrlProperty,
                GenerateClientClasses = settings.GenerateClientClasses,
                GenerateClientInterfaces = settings.GenerateClientInterfaces,
                GenerateDtoTypes = settings.GenerateDtoTypes,
                GenerateExceptionClasses = settings.GenerateExceptionClasses,
                GeneratePrepareRequestAndProcessResponseAsAsyncMethods = settings.GeneratePrepareRequestAndProcessResponseAsAsyncMethods,
                GenerateResponseClasses = settings.GenerateResponseClasses,
                GenerateSyncMethods = settings.GenerateSyncMethods,
                GenerateUpdateJsonSerializerSettingsMethod = settings.GenerateUpdateJsonSerializerSettingsMethod,
            });

            return generator.GenerateFile();
        }

        #endregion
    }
}
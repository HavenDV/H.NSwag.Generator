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
            try
            {
                var file = context.AdditionalFiles
                               .FirstOrDefault(text =>
                                   text.Path.EndsWith(".nswag", StringComparison.InvariantCultureIgnoreCase))
                           ?? throw new InvalidOperationException(".nswag file is not found.");

                var code = Generate(file.Path);

                context.AddSource(nameof(NSwagGenerator), SourceText.From(code, Encoding.UTF8));
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

        public static string Generate(string nswagPath)
        {
            nswagPath = nswagPath ?? throw new ArgumentNullException(nameof(nswagPath));

            var json = File.ReadAllText(nswagPath);
            var document = 
                JsonConvert.DeserializeObject<NSwagDocument>(json) ??
                throw new InvalidOperationException("Document is null.");
            var settings = document.CodeGenerators.OpenApiToCSharpClient;
            
            var openApi = Task.Run(() => 
                string.IsNullOrWhiteSpace(document.DocumentGenerator.FromDocument.Url)
                    ? document.DocumentGenerator.FromDocument.Json.StartsWith("{", StringComparison.OrdinalIgnoreCase)
                        ? OpenApiDocument.FromJsonAsync(document.DocumentGenerator.FromDocument.Json)
                        : OpenApiYamlDocument.FromYamlAsync(document.DocumentGenerator.FromDocument.Json)
                    : OpenApiDocument.FromUrlAsync(document.DocumentGenerator.FromDocument.Url))
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
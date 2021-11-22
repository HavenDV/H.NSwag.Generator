using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace H.NSwag.Generator;

[Generator]
public class NSwagGenerator : ISourceGenerator
{
    #region Properties

    public Dictionary<string, string> Cache { get; } = new();

    #endregion

    #region Methods

    public void Execute(GeneratorExecutionContext context)
    {
        var useCache = bool.Parse(GetGlobalOption(context, "UseCache") ?? bool.TrueString);

        foreach (var text in context.AdditionalFiles
            .Where(static text => text.Path.EndsWith(
                ".nswag",
                StringComparison.InvariantCultureIgnoreCase)))
        {
            try
            {
                string source;
                if (useCache &&
                    Cache.TryGetValue(text.Path, out var value))
                {
                    source = value;
                }
                else
                {
                    source = Task.Run(() => GenerateAsync(
                        text.Path,
                        context.CancellationToken)).Result;
                    Cache[text.Path] = source;
                }

                context.AddSource(
                    $"{Path.GetFileName(text.Path)}.cs",
                    SourceText.From(source, Encoding.UTF8));
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

    private static async Task<OpenApiDocument> GetOpenApiDocumentAsync(
        FromDocument fromDocument,
        string path,
        CancellationToken cancellationToken = default)
    {
        var folder = Path.GetDirectoryName(path) ?? string.Empty;

        var fromUrl = !string.IsNullOrWhiteSpace(fromDocument.Url);
        var fromFile = fromUrl && !fromDocument.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        if (fromUrl && !fromFile)
        {
            return await OpenApiDocument.FromUrlAsync(
                fromDocument.Url,
                cancellationToken).ConfigureAwait(false);
        }

        var json = fromFile
            ? File.ReadAllText(Path.Combine(folder, fromDocument.Url))
            : fromDocument.Json;
        var isJson = json.StartsWith("{", StringComparison.OrdinalIgnoreCase);

        return isJson
            ? await OpenApiDocument.FromJsonAsync(json, cancellationToken).ConfigureAwait(false)
            : await OpenApiYamlDocument.FromYamlAsync(json, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> GenerateAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        path = path ?? throw new ArgumentNullException(nameof(path));

        var json = File.ReadAllText(path);
        var document =
            JsonConvert.DeserializeObject<NSwagDocument>(json) ??
            throw new InvalidOperationException("Document is null.");
        var settings = document.CodeGenerators.OpenApiToCSharpClient;
        var openApi = await GetOpenApiDocumentAsync(
            document.DocumentGenerator.FromDocument,
            path,
            cancellationToken).ConfigureAwait(false);

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
                GenerateOptionalPropertiesAsNullable = settings.GenerateOptionalPropertiesAsNullable,
                GenerateDataAnnotations = settings.GenerateDataAnnotations,
                GenerateDefaultValues = settings.GenerateDefaultValues,
                GenerateImmutableArrayProperties = settings.GenerateImmutableArrayProperties,
                GenerateImmutableDictionaryProperties = settings.GenerateImmutableDictionaryProperties,
                GenerateJsonMethods = settings.GenerateJsonMethods,
                EnforceFlagEnums = settings.EnforceFlagEnums,
                ExcludedTypeNames = settings.ExcludedTypeNames,
                DictionaryInstanceType = settings.DictionaryInstanceType,
                DictionaryType = settings.DictionaryType,
                HandleReferences = settings.HandleReferences,
                InlineNamedAny = settings.InlineNamedAny,
                InlineNamedArrays = settings.InlineNamedArrays,
                InlineNamedDictionaries = settings.InlineNamedDictionaries,
                InlineNamedTuples = settings.InlineNamedTuples,
                RequiredPropertiesMustBeDefined = settings.RequiredPropertiesMustBeDefined,
                TypeAccessModifier = settings.TypeAccessModifier,
                TimeType = settings.TimeType,
                TemplateDirectory = settings.TemplateDirectory,
                TimeSpanType = settings.TimeSpanType,
                JsonConverters = settings.JsonConverters,
                AnyType = settings.AnyType,
                ArrayBaseType = settings.ArrayBaseType,
                ArrayInstanceType = settings.ArrayInstanceType,
                ArrayType = settings.ArrayType,
                ClassStyle = settings.ClassStyle,
                DateTimeType = settings.DateTimeType,
                DateType = settings.DateType,
                DictionaryBaseType = settings.DictionaryBaseType,
            },
            CodeGeneratorSettings =
            {
                ExcludedTypeNames = settings.ExcludedTypeNames,
                InlineNamedAny = settings.InlineNamedAny,
                GenerateDefaultValues = settings.GenerateDefaultValues,
                TemplateDirectory = settings.TemplateDirectory,
            },
            UseBaseUrl = settings.UseBaseUrl,
            UseHttpClientCreationMethod = settings.UseHttpClientCreationMethod,
            UseHttpRequestMessageCreationMethod = settings.UseHttpRequestMessageCreationMethod,
            UseRequestAndResponseSerializationSettings = settings.UseRequestAndResponseSerializationSettings,
            WrapDtoExceptions = settings.WrapDtoExceptions,
            WrapResponseMethods = settings.WrapResponseMethods,
            WrapResponses = settings.WrapResponses,
            ParameterArrayType = settings.ParameterArrayType,
            ParameterDateFormat = settings.ParameterDateFormat,
            ParameterDateTimeFormat = settings.ParameterDateTimeFormat,
            ParameterDictionaryType = settings.ParameterDictionaryType,
            InjectHttpClient = settings.InjectHttpClient,
            QueryNullValue = settings.QueryNullValue,
            HttpClientType = settings.HttpClientType,
            ResponseArrayType = settings.ResponseArrayType,
            ResponseClass = settings.ResponseClass,
            ResponseDictionaryType = settings.ResponseDictionaryType,
            SerializeTypeInformation = settings.SerializeTypeInformation,
            ExposeJsonSerializerSettings = settings.ExposeJsonSerializerSettings,
            ExcludedParameterNames = settings.ExcludedParameterNames,
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

    #region Utilities

    private static string? GetGlobalOption(GeneratorExecutionContext context, string name)
    {
        return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            $"build_property.{nameof(NSwagGenerator)}_{name}",
            out var result) &&
            !string.IsNullOrWhiteSpace(result)
            ? result
            : null;
    }

    #endregion
}

using Microsoft.CodeAnalysis;

namespace H.Generators.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public Task GeneratesWithoutNSwagFilesCorrectly()
    {
        return this.CheckSourceAsync(
            Array.Empty<AdditionalText>());
    }

    [TestMethod]
    public Task GeneratesFromYamlCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromYamlCorrectly)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);
        
        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }

    [TestMethod]
    public Task GeneratesFromYamlFileCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromYamlFileCorrectly)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);

        File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "openapi.yaml"), Resources.openapi_yaml.AsBytes());

        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }

    [TestMethod]
    public Task GeneratesFromUrlCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromUrlCorrectly)}.nswag");
        var text = Resources.openapi_from_url_nswag.AsString();
        File.WriteAllText(path, text);

        return this.CheckSourceAsync(
            new AdditionalText[] { new CustomAdditionalText(path, text) });
    }
}
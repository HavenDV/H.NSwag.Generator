namespace H.Generators.IntegrationTests;

[TestClass]
public class NSwagGeneratorSnapshotTests : VerifyBase
{
    [TestMethod]
    public Task GeneratesWithoutNSwagFilesCorrectly()
    {
        return this.CheckSource();
    }

    [TestMethod]
    public Task GeneratesFromYamlCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromYamlCorrectly)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);
        
        return this.CheckSource(
            new CustomAdditionalText(path, text));
    }

    [TestMethod]
    public Task GeneratesFromYamlFileCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromYamlFileCorrectly)}.nswag");
        var text = Resources.openapi_from_yaml_nswag.AsString();
        File.WriteAllText(path, text);

        File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "openapi.yaml"), Resources.openapi_yaml.AsBytes());

        return this.CheckSource(
            new CustomAdditionalText(path, text));
    }

    [TestMethod]
    public Task GeneratesFromUrlCorrectly()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(GeneratesFromUrlCorrectly)}.nswag");
        var text = Resources.openapi_from_url_nswag.AsString();
        File.WriteAllText(path, text);

        return this.CheckSource(
            new CustomAdditionalText(path, text));
    }
}
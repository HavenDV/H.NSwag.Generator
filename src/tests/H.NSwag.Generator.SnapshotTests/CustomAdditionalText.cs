using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace H.Generators.IntegrationTests;

public class CustomAdditionalText : AdditionalText
{
    public string Text { get; }

    public override string Path { get; }

    public CustomAdditionalText(string path, string text)
    {
        Path = path;
        Text = text;
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(Text);
    }
}
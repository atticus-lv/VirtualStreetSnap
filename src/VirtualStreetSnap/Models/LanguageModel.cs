namespace VirtualStreetSnap.Models;

public class LanguageModel
{
    public string Label { get; set; } = "English";
    public string Identifier { get; set; } = "en-US";

    public LanguageModel(string label, string identifier)
    {
        Label = label;
        Identifier = identifier;
    }
}
namespace Models;

public class Language
{
    public string Name { get; set; }
    
    public List<Text> Texts { get; init; } = new();

    
}
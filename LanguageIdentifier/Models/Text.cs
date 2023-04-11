using System.Text.RegularExpressions;

namespace Models;

public partial class Text
{
    private readonly string _rawText;
    public string? Lang { get; set; }
    private Dictionary<char, int> LetterDictionary { get; } = new();

    public Text(string text, string? lang)
    {
        _rawText = CleanUpText(text.ToLower());
        Lang = lang;
        GetLetterDictionary(_rawText);
    }

    public IEnumerable<double> GetCharCountVector() =>
        LetterDictionary.Values.ToList().Select(t => t / (double)_rawText.Length);

    private static string CleanUpText(string text) => ASCIIRegex().Replace(text, "");


    private void GetLetterDictionary(string text)
    {
        for (int i = 'a'; i <= 'z'; i++)
        {
            LetterDictionary.Add((char)i, 0);
        }

        var newDict = text.GroupBy(c => c).OrderBy(k => k.Key).ToDictionary(g => g.Key, g => g.Count());

        newDict.ToList().ForEach(t => LetterDictionary[t.Key] = t.Value);

    }


    [GeneratedRegex("[^A-Za-z]+")]
    private static partial Regex ASCIIRegex();
}
using System.Text.RegularExpressions;
using Models;

namespace LanguageIdentifier;

public class LanguagesContext
{
    public readonly List<Language> Languages = new();
    private readonly Dictionary<string, DataSet> LanguageDataSets = new();

    public void LoadLanguages(string dataPath)
    {
        if (!Directory.Exists(dataPath)) throw new DirectoryNotFoundException("data folder doesnt exists");

        Directory.GetDirectories(dataPath).ToList().ForEach(directory =>
        {
            var languageName = Path.GetFileName(directory);

            var language = new Language
            {
                Name = languageName
            };
            Directory.GetFiles(directory, "*.txt").ToList().ForEach(file =>
            {
                language.Texts.Add(new Text(File.ReadAllText(file), languageName));
            });
            Languages.Add(language);
            LanguageDataSets[languageName] = getDataSet();
        });
    }

    public DataSet getDataSet()
    {
        return new DataSet
        {
            VectorSize = 'z' - 'a' + 1,
            Data = Languages
                .SelectMany(l => l.Texts)
                .OrderBy(d => Guid.NewGuid())
                .Select(t => new Data
                {
                    Vector = t.GetCharCountVector().Select(Convert.ToDouble).ToList(),
                    Label = t.Lang
                }).ToList()
        };
    }
}
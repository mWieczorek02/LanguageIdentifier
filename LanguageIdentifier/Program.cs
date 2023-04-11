using LanguageIdentifier;
using Models;


const double learningRate = 0.5d;
var rng = new Random();

var trainLanguageContext = new LanguagesContext();
var testLanguageContext = new LanguagesContext();

trainLanguageContext.LoadLanguages(@"../../../Data");
testLanguageContext.LoadLanguages(@"../../../TestSet");

var trainSet = trainLanguageContext.getDataSet();
var testSet = testLanguageContext.getDataSet();
trainSet.Data = trainSet.Data.OrderBy(i => rng.Next()).ToList();

var perceptronLayer = trainLanguageContext.Languages.ToList()
    .Select(language => new Perceptron(trainSet, learningRate, language.Name)).ToList();


var perceptronTasks = new List<Task>();

perceptronLayer.ForEach(p =>
{
    Console.WriteLine("new task");
    var task = p.TrainUntil(1, trainSet, testSet);
    perceptronTasks.Add(task);
});

Task.WaitAll(perceptronTasks.ToArray());

while (true)
{
    Console.Write("input text: ");
    var text = Console.ReadLine();
    var predictions = new List<dynamic>();

    perceptronLayer.ForEach(p =>
    {
        var prediction = p.NormalizedNet(new Data
        {
            Vector = (new Text(text.ToLower(), null)).GetCharCountVector().Select(Convert.ToDouble).ToList()
        });

        Console.WriteLine($"{p.Classifier} accuracy: {prediction}");
        
        predictions.Add(new
        {
            Label = p.Classifier,
            Prediction = prediction
        });
    });

    var max = predictions.MaxBy(d => d.Prediction);
    Console.WriteLine($"{max.Label}");
}
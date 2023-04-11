namespace LanguageIdentifier;

using Interfaces;
using Models;

public class Perceptron : IPredictable
{
    public readonly string? Classifier;

    private double _threshold;
    private List<double> _weights;
    private readonly double _learningRate;

    public Perceptron(DataSet trainSet, double learningRate, string classifier)
    {
        Classifier = classifier;

        _learningRate = learningRate;

        var r = new Random();

        _threshold = r.NextDouble() * (1 - -1) + -1;
        _weights = Enumerable.Range(0, trainSet.VectorSize ?? 0).Select(_ => r.NextDouble() * (1 - -1) + -1).ToList();

        Train(trainSet);
    }

    public bool Predict(Data input)
    {
        var net = GetNet(input.Vector) + _threshold;
        return net >= 0;
    }

    public double NormalizedNet(Data input)
    {
        return Normalize(input.Vector).Zip(Normalize(_weights), (a, b) => a * b)
            .Aggregate(0d, (acc, val) => acc + val);
    }

    private void Train(DataSet trainSet) => trainSet.Data.ToList().ForEach(Train);

    public async Task TrainUntil(double accuracyTarget, DataSet trainSet, DataSet testSet)
    {
        await Task.Run(() =>
        {
            var rng = new Random();

            var max = new
            {
                Accuracy = 0d,
                Weights = new List<double>(),
                Treshold = 0d,
            };

            double accuracy = 0;
            var index = 0;

            while (accuracy < accuracyTarget)
            {
                if (index > 100) break;
                var correct = 0;
                trainSet.Data = trainSet.Data.OrderBy(i => rng.Next()).ToList();
                Train(trainSet);
                testSet.Data.OrderBy(i => rng.Next()).ToList().ForEach(d =>
                {
                    var isActive = NormalizedNet(d) >= 0;
                    var equalsToClassifier = Classifier == d.Label;
                    switch (isActive)
                    {
                        case true when equalsToClassifier:
                        case false when !equalsToClassifier:
                            correct++;
                            break;
                    }
                });
                accuracy = correct / (double)testSet.Data.Count;

                if (accuracy > max.Accuracy)
                {
                    max = new
                    {
                        Accuracy = accuracy,
                        Weights = _weights.ToList(),
                        Treshold = _threshold,
                    };
                }

                index++;
            }

            if (max.Accuracy is 0) return;
            _weights = max.Weights;
            _threshold = max.Treshold;
        });
    }

    private double GetNet(IEnumerable<double> vector) =>
        // net = X∘W-t
        vector.Zip(_weights, (a, b) => a * b).Aggregate(0d, (acc, val) => acc + val) - _threshold;


    private void Train(Data input)
    {
        var net = GetNet(input.Vector);
        var isActive = net >= 0 ? 1 : 0;
        var expected = input.Label == Classifier ? 1 : 0;


        //W’=W+(d-y)𝛼X
        _weights = _weights.Zip(input.Vector.Select(
                val => (expected - isActive) * _learningRate * val)
            , (a, b) => a + b).ToList();

        //t’=t-(d-y)𝛼
        _threshold -= (expected - isActive) * _learningRate;
    }

    private static IEnumerable<double> Normalize(List<double> data)
    {
        var vectorLength = Math.Sqrt(data.Aggregate(0d, ((i, d) => i + Math.Pow(d, 2))));

        return data.Select(d => d / vectorLength);
    }
}
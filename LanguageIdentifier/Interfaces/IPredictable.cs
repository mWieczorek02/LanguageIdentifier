namespace Interfaces;

using Models;


public interface IPredictable
{
    public bool Predict(Data input);
}
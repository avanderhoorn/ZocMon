using ZocMonLib;

namespace ZocMonLib
{
    public interface IReduceMethodProvider
    {
        void Register(IReduceMethod<double> instance);

        IReduceMethod<double> Retrieve(string name);
    }
}
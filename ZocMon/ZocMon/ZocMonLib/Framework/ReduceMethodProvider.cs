using System;
using System.Collections.Generic;
using ZocMonLib;

namespace ZocMonLib
{
    public class ReduceMethodProvider : IReduceMethodProvider
    {
        private readonly IDictionary<string, IReduceMethod<double>> _factories;

        public ReduceMethodProvider()
        {
            _factories = new Dictionary<string, IReduceMethod<double>>();
        }

        protected IDictionary<string, IReduceMethod<double>> Factories
        {
            get { return _factories; }
        }

        public void Register(IReduceMethod<double> instance)
        {
            _factories.Add(instance.GetType().FullName, instance);
        }

        public IReduceMethod<double> Retrieve(string name)
        {
            IReduceMethod<double> instance;
            if (!_factories.TryGetValue(name, out instance))
                throw new KeyNotFoundException(String.Format("Reduce type of {0} was not found.", name));
            return instance;
        }
    }
}
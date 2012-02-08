using System;
using System.Collections.Generic;
using ZocMonLib;

namespace ZocMonLib
{
    public class ProcessingInstructionProvider : IProcessingInstructionProvider
    {
        private readonly IDictionary<string, IProcessingInstruction> _factories;

        public ProcessingInstructionProvider()
        {
            _factories = new Dictionary<string, IProcessingInstruction>();
        }

        protected IDictionary<string, IProcessingInstruction> Factories
        {
            get { return _factories; }
        }

        public void Register(IProcessingInstruction instance)
        {
            _factories.Add(instance.GetType().FullName, instance);
        }

        public IProcessingInstruction Retrieve(string name)
        {
            IProcessingInstruction instance;
            if (!_factories.TryGetValue(name, out instance))
                throw new KeyNotFoundException(String.Format("Reduce type of {0} was not found.", name));
            return instance;
        }
    }
}
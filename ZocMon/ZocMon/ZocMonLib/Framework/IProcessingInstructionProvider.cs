using ZocMonLib;

namespace ZocMonLib
{
    public interface IProcessingInstructionProvider
    {
        void Register(IProcessingInstruction instance);

        IProcessingInstruction Retrieve(string name);
    }
}
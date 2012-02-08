namespace ZocMonLib
{
    public interface IRecordReduceStatusSourceProvider
    {
        string ReadValue();

        void WriteValue(string value);
    }
}
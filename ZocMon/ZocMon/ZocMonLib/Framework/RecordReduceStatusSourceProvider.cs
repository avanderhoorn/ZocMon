namespace ZocMonLib
{
    public abstract class RecordReduceStatusSourceProvider : IRecordReduceStatusSourceProvider
    {
        public abstract string ReadValue();

        public abstract void WriteValue(string value);

        protected string SeedValue()
        {
            return "1";
        }
    }
}
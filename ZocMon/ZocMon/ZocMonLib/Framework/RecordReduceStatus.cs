namespace ZocMonLib
{
    public class RecordReduceStatus : IRecordReduceStatus
    {
        private readonly IRecordReduceStatusSourceProvider _statusSourceProvider;

        public RecordReduceStatus(IRecordReduceStatusSourceProvider statusSourceProvider)
        {
            _statusSourceProvider = statusSourceProvider;
        }

        public bool IsReducing()
        {
            //Pull value out
            var status = _statusSourceProvider.ReadValue();

            //Isn't already reducing
            if (status == "0")
                _statusSourceProvider.WriteValue("1");

            return status == "1";
        }

        public void DoneReducing()
        {
            _statusSourceProvider.WriteValue("0");
        }
    }
}
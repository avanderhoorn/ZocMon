namespace ZocMonLib
{
    public interface IRecordReduceStatus
    {
        /// <summary>
        /// Whether we are currently reducing. If its not reducing it will set the reduce flag.
        /// </summary>
        /// <returns></returns>
        bool IsReducing();

        /// <summary>
        /// Tell the system that we are done reducing
        /// </summary>
        void DoneReducing();
    }
}
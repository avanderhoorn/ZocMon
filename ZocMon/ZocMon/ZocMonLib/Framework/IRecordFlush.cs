using System.Data;

namespace ZocMonLib
{
    public interface IRecordFlush
    {
        /// <summary>
        /// Flush all data accumulated thus far.
        /// </summary>
        void FlushAll();

        /// <summary>
        /// Flush data for the lowest reduce resolution for the given configuration.
        /// (The rest is only written on Reduce.)
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="conn"></param>
        void Flush(string configName, IDbConnection conn);
    }
}
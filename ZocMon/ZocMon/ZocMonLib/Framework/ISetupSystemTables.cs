using System.Data;

namespace ZocMonLib
{
    public interface ISetupSystemTables
    {
        void ValidateAndCreateDataTables(IDbConnection conn);
    }
}
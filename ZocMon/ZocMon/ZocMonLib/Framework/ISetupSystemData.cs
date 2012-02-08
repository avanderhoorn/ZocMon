using System.Data;

namespace ZocMonLib
{
    public interface ISetupSystemData
    {
        void LoadAndValidateData(IDbConnection conn);
    }
}
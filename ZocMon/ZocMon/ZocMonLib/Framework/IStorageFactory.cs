using System.Data;

namespace ZocMonLib
{
    public interface IStorageFactory
    { 
        IDbCommand CreateCommand();

        IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction transaction);
         
        IDbConnection CreateConnection();

        IDbDataParameter CreateParameter();

        IDbDataParameter CreateParameter(string name, DbType type);
    }
}
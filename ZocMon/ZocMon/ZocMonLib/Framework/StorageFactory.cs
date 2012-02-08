using System.Data;
using System.Data.Common;
using ZocMonLib.Extensibility;

namespace ZocMonLib
{
    public class StorageFactory : IStorageFactory
    {
        private DbProviderFactory _factory;

        public StorageFactory(DbProviderFactory factory)
        {
            _factory = factory;
        }
        
        public IDbCommand CreateCommand()
        {
            return _factory.CreateCommand();
        }

        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            var command = this.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Connection = connection;
            command.Transaction = transaction;

            return command;
        }

        public IDbConnection CreateConnection()
        {
            return _factory.CreateConnection();
        } 

        public IDbDataParameter CreateParameter()
        {
            return _factory.CreateParameter();
        }

        public IDbDataParameter CreateParameter(string name, DbType type)
        {
            var param = this.CreateParameter();
            param.DbType = type;
            param.ParameterName = name;

            return param;
        }
    }
}
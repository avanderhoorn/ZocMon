using System;
using System.Collections.Generic;
using System.Data; 

namespace ZocMonLib.Plumbing
{

    public static class ZocMonSqlHelper
    {
        public static int ExecuteScalarWithConnection<T>(IDbConnection conn, string sql)
        {
            throw new NotImplementedException();
        }

        public static void ExecuteNonQueryWithConnection(IDbConnection conn, string command)
        {
        }

        public static int ExecuteNonQueryWithConnection(IDbConnection conn, string updateSql, MonitorRecord<double> update)
        {
            throw new NotImplementedException();
        }

        public static List<T> CreateListWithConnection<T>(IDbConnection conn, string sql)
        {
            throw new NotImplementedException();
        }

        public static List<T> CreateListWithConnection<T>(IDbConnection conn, string sql, object p)
        {
            throw new NotImplementedException();
        }

        public static List<T> CreateListWithConnection<T>(IDbConnection conn, string sql, object p, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public static void ExecuteNonQueryWithConnection(IDbConnection conn, string MonitorConfigInsert, object p, IDbTransaction transaction)
        { 
        }

        public static void ExecuteNonQueryWithConnection(IDbConnection conn, string sql, object p)
        { 
        }

        public static void InsertRecordWithConnection(IDbConnection conn, object o, object p, string targetReducedTableName)
        {
        }
    }

}

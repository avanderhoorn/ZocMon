using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace ZocMonLib
{
    public static class ZocMonSqlHelper
    {
        public static int ExecuteScalarWithConnection(IDbConnection conn, string sql)
        {
            return conn.Execute(sql);
        }

        public static IEnumerable<T> CreateListWithConnection<T>(IDbConnection conn, string sql)
        {
            return conn.Query<T>(sql);
        }

        public static IEnumerable<T> CreateListWithConnection<T>(IDbConnection conn, string sql, IDbTransaction transaction)
        {
            return conn.Query<T>(sql, transaction: transaction);
        }

        public static int ExecuteNonQueryWithConnection(IDbConnection conn, string sql)
        {
            return conn.Execute(sql);
        }

        public static int ExecuteNonQueryWithConnection(IDbConnection conn, string sql, object p)
        {
            return conn.Execute(sql, p);
        }

        public static int ExecuteNonQueryWithConnection(IDbConnection conn, string sql, object p, IDbTransaction transaction)
        {
            return conn.Execute(sql, p, transaction);
        }
         
        public static void InsertRecordWithConnection<T>(IDbConnection conn, T o, string targetReducedTableName) where T : class
        {
            conn.Insert(o);
        }
    }

}

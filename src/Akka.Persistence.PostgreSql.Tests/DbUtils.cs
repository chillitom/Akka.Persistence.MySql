using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.PostgreSql.Tests
{
    public static class DbUtils
    {
        public static void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var connectionBuilder = new MySqlConnectionStringBuilder(connectionString);

            //connect to postgres database to create a new database
            var databaseName = connectionBuilder.Database;
            connectionBuilder.Database = "INFORMATION_SCHEMA";
            connectionString = connectionBuilder.ToString();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                bool dbExists;
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT TRUE FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", databaseName);
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                    dbExists = result != null && Convert.ToBoolean(result);
                }

                if (dbExists)
                {
                    DoClean(conn, databaseName);
                }
                else
                {
                    DoCreate(conn, databaseName);
                }
            }
        }

        public static void Clean()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var connectionBuilder = new MySqlConnectionStringBuilder(connectionString);

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                DoClean(conn, connectionBuilder.Database);
            }
        }

        private static void DoCreate(MySqlConnection conn, string databaseName)
        {
            using (var cmd = new MySqlCommand())
            {
                cmd.CommandText = string.Format(@"CREATE DATABASE {0}", databaseName);
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }

        private static void DoClean(MySqlConnection conn, string databaseName)
        {
            using (var cmd = new MySqlCommand())
            {
                cmd.CommandText = string.Format(@"
                    DROP TABLE IF EXISTS {0}.event_journal;
                    DROP TABLE IF EXISTS {0}.snapshot_store", databaseName);
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
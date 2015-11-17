﻿using System;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.PostgreSql
{
    internal static class PostgreSqlInitializer
    {
        private const string SqlJournalFormat = @"
            CREATE TABLE IF NOT EXISTS {0}.{1} (
                persistence_id VARCHAR(200) NOT NULL,
                sequence_nr BIGINT NOT NULL,
                is_deleted BOOLEAN NOT NULL,
                payload_type VARCHAR(500) NOT NULL,
                payload BLOB NOT NULL,
                PRIMARY KEY (persistence_id, sequence_nr),
                INDEX (sequence_nr)
            );
            ";

        private const string SqlSnapshotStoreFormat = @"
            CREATE TABLE IF NOT EXISTS {0}.{1} (
                persistence_id VARCHAR(200) NOT NULL,
                sequence_nr BIGINT NOT NULL,
                created_at TIMESTAMP NOT NULL,
                created_at_ticks INT NOT NULL,
                snapshot_type VARCHAR(500) NOT NULL,
                `snapshot` BLOB NOT NULL,
                PRIMARY KEY (persistence_id, sequence_nr),
                INDEX (sequence_nr),
                INDEX (created_at)
            );
            ";

        /// <summary>
        /// Initializes a PostgreSQL journal-related tables according to 'schema-name', 'table-name' 
        /// and 'connection-string' values provided in 'akka.persistence.journal.postgresql' config.
        /// </summary>
        internal static void CreatePostgreSqlJournalTables(string connectionString, string schemaName, string tableName)
        {
            var sql = InitJournalSql(tableName, schemaName);
            ExecuteSql(connectionString, sql);
        }

        /// <summary>
        /// Initializes a PostgreSQL snapshot store related tables according to 'schema-name', 'table-name' 
        /// and 'connection-string' values provided in 'akka.persistence.snapshot-store.postgresql' config.
        /// </summary>
        internal static void CreatePostgreSqlSnapshotStoreTables(string connectionString, string schemaName, string tableName)
        {
            var sql = InitSnapshotStoreSql(tableName, schemaName);
            ExecuteSql(connectionString, sql);
        }

        private static string InitJournalSql(string tableName, string schemaName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "Akka.Persistence.PostgreSql journal table name is required");

            return SqlJournalFormat.QuoteSchemaAndTable(schemaName, tableName);
        }

        private static string InitSnapshotStoreSql(string tableName, string schemaName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "Akka.Persistence.PostgreSql snapshot store table name is required");

            return SqlSnapshotStoreFormat.QuoteSchemaAndTable(schemaName, tableName);
        }

        private static void ExecuteSql(string connectionString, string sql)
        {
            using (var conn = new MySqlConnection(connectionString))
            using (var command = conn.CreateCommand())
            {
                conn.Open();

                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }
    }
}
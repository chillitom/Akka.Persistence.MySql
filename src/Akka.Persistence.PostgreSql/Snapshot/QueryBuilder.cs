using System;
using System.Text;

using Akka.Persistence.Sql.Common.Snapshot;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.PostgreSql.Snapshot
{
    internal class PostgreSqlSnapshotQueryBuilder : ISnapshotQueryBuilder
    {
        private readonly string _deleteSql;
        private readonly string _insertSql;
        private readonly string _selectSql;

        public PostgreSqlSnapshotQueryBuilder(string schemaName, string tableName)
        {
            _deleteSql = @"DELETE FROM {0}.{1} WHERE persistence_id = @persistence_id ".QuoteSchemaAndTable(schemaName, tableName);
            _insertSql = @"INSERT INTO {0}.{1} (persistence_id, sequence_nr, created_at, created_at_ticks, snapshot_type, snapshot) VALUES (@persistence_id, @sequence_nr, @created_at, @created_at_ticks, @snapshot_type, @snapshot)".QuoteSchemaAndTable(schemaName, tableName);
            _selectSql = @"SELECT persistence_id, sequence_nr, created_at, created_at_ticks, snapshot_type, snapshot FROM {0}.{1} WHERE persistence_id = @persistence_id".QuoteSchemaAndTable(schemaName, tableName);
        }

        public DbCommand DeleteOne(string persistenceId, long sequenceNr, DateTime timestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });
            var sb = new StringBuilder(_deleteSql);

            if (sequenceNr < long.MaxValue && sequenceNr > 0)
            {
                sb.Append(@"AND sequence_nr = @sequence_nr ");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64) {Value = sequenceNr});
            }

            if (timestamp > DateTime.MinValue && timestamp < DateTime.MaxValue)
            {
                sb.Append(@"AND created_at = @created_at AND created_at_ticks = @created_at_ticks");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Timestamp)
                {
                    Value = GetMaxPrecisionTicks(timestamp)
                });
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at_ticks", MySqlDbType.Int32)
                {
                    Value = GetExtraTicks(timestamp)
                });
            }

            sqlCommand.CommandText = sb.ToString();

            return sqlCommand;
        }

        public DbCommand DeleteMany(string persistenceId, long maxSequenceNr, DateTime maxTimestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });
            var sb = new StringBuilder(_deleteSql);

            if (maxSequenceNr < long.MaxValue && maxSequenceNr > 0)
            {
                sb.Append(@" AND sequence_nr <= @sequence_nr ");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64)
                {
                    Value = maxSequenceNr
                });
            }

            if (maxTimestamp > DateTime.MinValue && maxTimestamp < DateTime.MaxValue)
            {
                sb.Append(
                    @" AND (created_at < @created_at OR (created_at = @created_at AND created_at_ticks <= @created_at_ticks)) ");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Timestamp)
                {
                    Value = GetMaxPrecisionTicks(maxTimestamp)
                });
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at_ticks", MySqlDbType.Int32)
                {
                    Value = GetExtraTicks(maxTimestamp)
                });
            }

            sqlCommand.CommandText = sb.ToString();

            return sqlCommand;
        }

        public DbCommand InsertSnapshot(SnapshotEntry entry)
        {
            var sqlCommand = new MySqlCommand(_insertSql)
            {
                Parameters =
                {
                    new MySqlParameter("@persistence_id", MySqlDbType.VarChar, entry.PersistenceId.Length) { Value = entry.PersistenceId },
                    new MySqlParameter("@sequence_nr", MySqlDbType.Int64) { Value = entry.SequenceNr },
                    new MySqlParameter("@created_at", MySqlDbType.Timestamp) { Value = GetMaxPrecisionTicks(entry.Timestamp) },
                    new MySqlParameter("@created_at_ticks", MySqlDbType.Int32) { Value = GetExtraTicks(entry.Timestamp) },
                    new MySqlParameter("@snapshot_type", MySqlDbType.VarChar, entry.SnapshotType.Length) { Value = entry.SnapshotType },
                    new MySqlParameter("@snapshot", MySqlDbType.Blob, entry.Snapshot.Length) { Value = entry.Snapshot }
                }
            };

            return sqlCommand;
        }

        public DbCommand SelectSnapshot(string persistenceId, long maxSequenceNr, DateTime maxTimestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });

            var sb = new StringBuilder(_selectSql);
            if (maxSequenceNr > 0 && maxSequenceNr < long.MaxValue)
            {
                sb.Append(" AND sequence_nr <= @sequence_nr ");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64)
                {
                    Value = maxSequenceNr
                });
            }

            if (maxTimestamp > DateTime.MinValue && maxTimestamp < DateTime.MaxValue)
            {
                sb.Append(
                    @" AND (created_at < @created_at OR (created_at = @created_at AND created_at_ticks <= @created_at_ticks)) ");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Timestamp)
                {
                    Value = GetMaxPrecisionTicks(maxTimestamp)
                });
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at_ticks", MySqlDbType.Int32)
                {
                    Value = GetExtraTicks(maxTimestamp)
                });
            }

            sb.Append(" ORDER BY sequence_nr DESC");
            sqlCommand.CommandText = sb.ToString();
            return sqlCommand;
        }

        private static DateTime GetMaxPrecisionTicks(DateTime date)
        {
            return new DateTime((date.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
        }

        private static int GetExtraTicks(DateTime date)
        {
            var ticks = date.Ticks;

            return (int)(ticks % TimeSpan.TicksPerSecond);
        }
    }
}
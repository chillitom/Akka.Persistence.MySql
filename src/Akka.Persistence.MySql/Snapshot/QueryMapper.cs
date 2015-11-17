﻿using System;
using System.Data.Common;
using Akka.Persistence.Sql.Common.Snapshot;

namespace Akka.Persistence.MySql.Snapshot
{
    internal class MySqlSnapshotQueryMapper : ISnapshotQueryMapper
    {
        private readonly Akka.Serialization.Serialization _serialization;

        public MySqlSnapshotQueryMapper(Akka.Serialization.Serialization serialization)
        {
            _serialization = serialization;
        }

        public SelectedSnapshot Map(DbDataReader reader)
        {
            var persistenceId = reader.GetString(0);
            var sequenceNr = reader.GetInt64(1);

            var timestamp = reader.GetDateTime(2);
            var timestampTicks = reader.GetInt32(3);
            timestamp = timestamp.AddTicks(timestampTicks);

            var metadata = new SnapshotMetadata(persistenceId, sequenceNr, timestamp);
            var snapshot = GetSnapshot(reader);

            return new SelectedSnapshot(metadata, snapshot);
        }

        private object GetSnapshot(DbDataReader reader)
        {
            var type = Type.GetType(reader.GetString(4), true);
            var serializer = _serialization.FindSerializerForType(type);
            var binary = (byte[])reader[5];

            var obj = serializer.FromBinary(binary, type);

            return obj;
        }
    }
}
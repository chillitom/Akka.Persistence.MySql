using Akka.Persistence.Sql.Common.Snapshot;
using Akka.Persistence.Sql.Common;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Snapshot
{
    /// <summary>
    /// Actor used for storing incoming snapshots into persistent snapshot store backed by MySql database.
    /// </summary>
    public class MySqlSnapshotStore : DbSnapshotStore
    {
        private readonly MySqlPersistenceExtension _extension;
        private readonly MySqlSnapshotStoreSettings _settings;

        public MySqlSnapshotStore()
        {
            _extension = MySqlPersistence.Instance.Apply(Context.System);

            _settings = _extension.SnapshotStoreSettings;
            QueryBuilder = new MySqlSnapshotQueryBuilder(_settings.SchemaName, _settings.TableName);
            QueryMapper = new MySqlSnapshotQueryMapper(Context.System.Serialization);
        }

        protected override SnapshotStoreSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        protected override DbConnection CreateDbConnection()
        {
            return new MySqlConnection(Settings.ConnectionString);
        }
    }
}
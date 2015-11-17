using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.TestKit.Snapshot;

namespace Akka.Persistence.MySql.Tests
{
    public class MySqlSnapshotStoreSpec : SnapshotStoreSpec
    {
        private static readonly Config SpecConfig;

        static MySqlSnapshotStoreSpec()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            var config = @"
                akka.persistence {
                    publish-plugin-commands = on
                    snapshot-store {
                        plugin = ""akka.persistence.snapshot-store.MySql""
                        MySql {
                            class = ""Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = snapshot_store
                            schema-name = akka_persistence_tests
                            auto-initialize = on
                            connection-string = """ + connectionString + @"""
                        }
                    }
                }";

            SpecConfig = ConfigurationFactory.ParseString(config);

            //need to make sure db is created before the tests start
            DbUtils.Initialize();
        }

        public MySqlSnapshotStoreSpec()
            : base(SpecConfig, "MySqlSnapshotStoreSpec")
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
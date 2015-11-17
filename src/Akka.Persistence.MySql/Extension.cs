using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Sql.Common;

namespace Akka.Persistence.MySql
{
    /// <summary>
    /// Configuration settings representation targeting MySql journal actor.
    /// </summary>
    public class MySqlJournalSettings : JournalSettings
    {
        public const string JournalConfigPath = "akka.persistence.journal.mysql";

        /// <summary>
        /// Flag determining in case of event journal table missing, it should be automatically initialized.
        /// </summary>
        public bool AutoInitialize { get; private set; }

        public MySqlJournalSettings(Config config)
            : base(config)
        {
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    /// <summary>
    /// Configuration settings representation targeting MySql snapshot store actor.
    /// </summary>
    public class MySqlSnapshotStoreSettings : SnapshotStoreSettings
    {
        public const string SnapshotStoreConfigPath = "akka.persistence.snapshot-store.mysql";

        /// <summary>
        /// Flag determining in case of snapshot store table missing, it should be automatically initialized.
        /// </summary>
        public bool AutoInitialize { get; private set; }

        public MySqlSnapshotStoreSettings(Config config)
            : base(config)
        {
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    /// <summary>
    /// An actor system extension initializing support for MySql persistence layer.
    /// </summary>
    public class MySqlPersistenceExtension : IExtension
    {
        /// <summary>
        /// Journal-related settings loaded from HOCON configuration.
        /// </summary>
        public readonly MySqlJournalSettings JournalSettings;

        /// <summary>
        /// Snapshot store related settings loaded from HOCON configuration.
        /// </summary>
        public readonly MySqlSnapshotStoreSettings SnapshotStoreSettings;

        public MySqlPersistenceExtension(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(MySqlPersistence.DefaultConfiguration());

            JournalSettings = new MySqlJournalSettings(system.Settings.Config.GetConfig(MySqlJournalSettings.JournalConfigPath));
            SnapshotStoreSettings = new MySqlSnapshotStoreSettings(system.Settings.Config.GetConfig(MySqlSnapshotStoreSettings.SnapshotStoreConfigPath));

            if (JournalSettings.AutoInitialize)
            {
                MySqlInitializer.CreateMySqlJournalTables(JournalSettings.ConnectionString, JournalSettings.SchemaName, JournalSettings.TableName);
            }

            if (SnapshotStoreSettings.AutoInitialize)
            {
                MySqlInitializer.CreateMySqlSnapshotStoreTables(SnapshotStoreSettings.ConnectionString, SnapshotStoreSettings.SchemaName, SnapshotStoreSettings.TableName);
            }
        }
    }

    /// <summary>
    /// Singleton class used to setup MySql backend for akka persistence plugin.
    /// </summary>
    public class MySqlPersistence : ExtensionIdProvider<MySqlPersistenceExtension>
    {
        public static readonly MySqlPersistence Instance = new MySqlPersistence();

        /// <summary>
        /// Initializes a MySql persistence plugin inside provided <paramref name="actorSystem"/>.
        /// </summary>
        public static void Init(ActorSystem actorSystem)
        {
            Instance.Apply(actorSystem);
        }

        private MySqlPersistence() { }
        
        /// <summary>
        /// Creates an actor system extension for akka persistence MySql support.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override MySqlPersistenceExtension CreateExtension(ExtendedActorSystem system)
        {
            return new MySqlPersistenceExtension(system);
        }

        /// <summary>
        /// Returns a default configuration for akka persistence MySql-based journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<MySqlPersistence>("Akka.Persistence.MySql.mysql.conf");
        }
    }
}
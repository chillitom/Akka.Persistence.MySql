using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.TestKit.Journal;

namespace Akka.Persistence.MySql.Tests
{
    public class MySqlJournalSpec : JournalSpec
    {
        private static readonly Config SpecConfig;

        static MySqlJournalSpec()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            var config = @"
                akka.persistence {
                    publish-plugin-commands = on
                    journal {
                        plugin = ""akka.persistence.journal.MySql""
                        MySql {
                            class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = event_journal
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

        public MySqlJournalSpec()
            : base(SpecConfig, "MySqlJournalSpec")
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
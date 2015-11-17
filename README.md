## Akka.Persistence.MySql

Akka Persistence journal and snapshot store backed by MySql database.

**WARNING: Akka.Persistence.MySql plugin is still in beta and it's mechanics described below may be still subject to change**.

### Setup

To activate the journal plugin, add the following lines to actor system configuration file:

```
akka.persistence.journal.plugin = "akka.persistence.journal.mysql"
akka.persistence.journal.mysql.connection-string = "<database connection string>"
```

Similar configuration may be used to setup a MySql snapshot store:

```
akka.persistence.snapshot-store.plugin = "akka.persistence.snapshot-store.mysql"
akka.persistence.snapshot-store.mysql.connection-string = "<database connection string>"
```

Remember that connection string must be provided separately to Journal and Snapshot Store. To finish setup simply initialize plugin using: `MySqlPersistence.Init(actorSystem);`

### Configuration

Both journal and snapshot store share the same configuration keys (however they resides in separate scopes, so they are definied distinctly for either journal or snapshot store):

- `class` (string with fully qualified type name) - determines class to be used as a persistent journal. Default: *Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql* (for journal) and *Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql* (for snapshot store).
- `plugin-dispatcher` (string with configuration path) - describes a message dispatcher for persistent journal. Default: *akka.actor.default-dispatcher*
- `connection-string` - connection string used to access MySql database. Default: *none*.
- `connection-timeout` - timespan determining default connection timeouts on database-related operations. Default: *30s*
- `schema-name` - name of the database schema, where journal or snapshot store tables should be placed. Default: *public*
- `table-name` - name of the table used by either journal or snapshot store. Default: *event_journal* (for journal) or *snapshot_store* (for snapshot store)
- `auto-initialize` - flag determining if journal or snapshot store related tables should by automatically created when they have not been found in connected database. Default: *false*

### Custom SQL data queries

MySql persistence plugin defines a default table schema used for both journal and snapshot store.

**EventJournal table**:

    +----------------+-------------+------------+---------------+---------+
    | persistence_id | sequence_nr | is_deleted | payload_type  | payload |
    +----------------+-------------+------------+---------------+---------+
    | varchar(200)   | bigint      | boolean    | varchar(500)  | blob    |
    +----------------+-------------+------------+---------------+---------+
 
**SnapshotStore table**:
 
    +----------------+--------------+--------------------------+------------------+---------------+----------+
    | persistence_id | sequence_nr  | created_at               | created_at_ticks | snapshot_type | snapshot |
    +----------------+--------------+--------------------------+------------------+--------------------------+
    | varchar(200)   | bigint       | timestamp                | int              | varchar(500)  | blob     |
    +----------------+--------------+--------------------------+------------------+--------------------------+

**created_at and created_at_ticks - The max precision of a MySql timestamp prior to version 5.6.4 is one second. The max precision of a .Net DateTime object is a microsecond. Because of this differences, the additional ticks are saved in a separate column and combined during deserialization. There is also a check constraint restricting created_at_ticks to the range [0,10) to ensure that there are no precision differences in the opposite direction.**

Underneath Akka.Persistence.MySql uses the MySql.Data library to communicate with the database. You may choose not to use a dedicated built in ones, but to create your own being better fit for your use case. To do so, you have to create your own versions of `IJournalQueryBuilder` and `IJournalQueryMapper` (for custom journals) or `ISnapshotQueryBuilder` and `ISnapshotQueryMapper` (for custom snapshot store) and then attach inside journal, just like in the example below:

```csharp
class MyCustomMySqlJournal: Akka.Persistence.MySql.Journal.MySqlJournal 
{
    public MyCustomMySqlJournal() : base() 
    {
        QueryBuilder = new MyCustomJournalQueryBuilder();
        QueryMapper = new MyCustomJournalQueryMapper();
    }
}
```

The final step is to setup your custom journal using akka config:

```
akka.persistence.journal.mysql.class = "MyModule.MyCustomMySqlJournal, MyModule"
```

### Tests

The MySql tests are packaged and run as part of the default "All" build task.

In order to run the tests, you must do the following things:

1. Download and install MySql from: http://www.mysql.org/download/
2. Install MySql with the default settings.
3. A custom app.config file can be used and needs to be placed in the same folder as the dll
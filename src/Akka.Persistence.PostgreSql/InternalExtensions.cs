using MySql.Data.MySqlClient;

namespace Akka.Persistence.PostgreSql
{
    internal static class InternalExtensions
    {
        public static string QuoteSchemaAndTable(this string sqlQuery, string schemaName, string tableName)
        {
            var cb = new MySqlCommandBuilder();
            return string.Format(sqlQuery, cb.QuoteIdentifier(schemaName), cb.QuoteIdentifier(tableName));
        }
    }
}
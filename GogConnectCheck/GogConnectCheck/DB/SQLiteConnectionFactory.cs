using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;

namespace GogConnectCheck.DB
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public DbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }
    }
}

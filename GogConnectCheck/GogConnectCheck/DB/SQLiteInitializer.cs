using SQLite.CodeFirst;
using System;
using System.Data.Entity;

namespace GogConnectCheck.DB
{
    public class SQLiteInitializer : SqliteDropCreateDatabaseWhenModelChanges<SQLiteContext>
    {
        public SQLiteInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, typeof(ModelHistory))
        { }

        protected override void Seed(SQLiteContext context)
        {
        }
    }

    public class ModelHistory : IHistory
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Context { get; set; }
        public DateTime CreateDate { get; set; }
    }
}

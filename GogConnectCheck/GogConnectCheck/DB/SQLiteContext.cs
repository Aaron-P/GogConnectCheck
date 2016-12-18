using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;

namespace GogConnectCheck.DB
{
    public class SQLiteContext : DbContext
    {
        public SQLiteContext(string path)
            : base(String.Format(CultureInfo.InvariantCulture, @"Data Source={0}", path))
        {
            Configuration.LazyLoadingEnabled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(_ => _.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Database.SetInitializer(new SQLiteInitializer(modelBuilder));
        }

        public DbSet<Product> Products { get; set; }
    }
}

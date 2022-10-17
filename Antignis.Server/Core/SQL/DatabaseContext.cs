using Antignis.Server.Core.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;

namespace Antignis.Server.Core.SQL
{
    internal class DatabaseContext : DbContext
    {
        public DatabaseContext(string dbLocation) :
            base(new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = dbLocation,
                    ForeignKeys = true
                }.ConnectionString
            }, true)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Host> Host { get; set; }
        public DbSet<FileShare> FileShare { get; set; }
        public DbSet<TCPConnection> TCPConnection { get; set; }
        public DbSet<WindowsFirewallSetting> WindowsFirewallSetting { get; set; }
        public DbSet<WindowsFirewallRule> WindowsFirewallRule { get; set; }
        public DbSet<Port> Port { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Models.Program> Program { get; set; }

        public DbSet<Query> Query { get; set; }
    }
}

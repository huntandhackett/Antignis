using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Antignis.Server.Core.SQL
{
    public class Database
    {

        #region fields and properties

        /// <summary>
        /// Indicator if transaction against database is running
        /// </summary>
        private bool IsTransaction = false;

        /// <summary>
        /// Sqlite Transaction object
        /// </summary>
        private SQLiteTransaction transaction { get; set; }

        /// <summary>
        /// Location to the database. File does not have to exist
        /// </summary>
        private string dbLocation { get; set; }

        /// <summary>
        /// Connection string to connect to the sqlite database
        /// </summary>
        private string connectionString { get; set; }

        /// <summary>
        /// SqliteConnection object
        /// </summary>
        private SQLiteConnection conn { get; set; }

        private readonly Dictionary<string, int> computers = new Dictionary<string, int>();
        private readonly Dictionary<string, int> shares = new Dictionary<string, int>();
        private readonly Dictionary<string, int> roles = new Dictionary<string, int>();
        private readonly Dictionary<string, int> neighbors = new Dictionary<string, int>();
        private readonly Dictionary<int, int> ports = new Dictionary<int, int>();

        #endregion

        #region public methods

        /// <summary>
        /// Flag to indicate if the database has been made from scratch
        /// </summary>
        public bool isNew { get; set; }

        /// <summary>
        /// Flag to indicate whether this class has been initiated
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="databaseLocation"></param>
        public Database(string databaseLocation)
        {
            dbLocation = databaseLocation;

            //Initialize the database
            Init();

            IsConnected = true;
        }

        /// <summary>
        /// Runs query on the database and returns list with string from first column
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<string> QueryDatabase(string query, string[] paramNames, object[] paramValues)
        {
            List<string> result = new List<string>();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            SQLiteCommand sqlCommand = new SQLiteCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandText = query;

            // Add parameters
            for (int i = 0; i < paramNames.Length; i++)
                sqlCommand.Parameters.AddWithValue(paramNames[i], paramValues[i]);

            DataSet dSet = new DataSet();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);
            adapter.Fill(dSet);

            if (adapter != null)
                adapter.Dispose();

            if (sqlCommand != null)
                sqlCommand.Dispose();

            if (dSet.Tables[0].Rows.Count > 0)
            {
                foreach (System.Data.DataRow r in dSet.Tables[0].Rows)
                {
                    result.Add(r.ItemArray[0].ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Runs query on the database and returns list with string from first column
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<string> QueryDatabase(string query)
        {
            List<string> result = new List<string>();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            SQLiteCommand sqlCommand = new SQLiteCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandText = query;

            DataSet dSet = new DataSet();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);
            adapter.Fill(dSet);

            if (adapter != null)
                adapter.Dispose();

            if (sqlCommand != null)
                sqlCommand.Dispose();

            if (dSet.Tables[0].Rows.Count > 0)
            {
                foreach (System.Data.DataRow r in dSet.Tables[0].Rows)
                {
                    result.Add(r.ItemArray[0].ToString());
                }
            }


            return result;
        }

        /// <summary>
        /// Queries the database and returns the dataset
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataSet QueryDatabaseDS(string query)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            SQLiteCommand sqlCommand = new SQLiteCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandText = query;

            DataSet dSet = new DataSet();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);
            adapter.Fill(dSet);

            if (adapter != null)
                adapter.Dispose();

            if (sqlCommand != null)
                sqlCommand.Dispose();

            return dSet;
        }

        /// <summary>
        /// Adds a new record to the database.
        /// If records exists, the existing record will be updated. 
        /// </summary>
        public void AddHostRecord(List<Models.Host> hosts)
        {
            using (DatabaseContext context = new DatabaseContext(dbLocation))
            {
                foreach (Models.Host entity in hosts)
                {
                    try
                    {
                        // Check if entry exists
                        Models.Host existing = context.Host.SingleOrDefault(h => h.DNSHostname == entity.DNSHostname);
                        if (existing == null)
                        {
                            context.Host.Add(entity);
                            //Console.WriteLine($"Added: {entity.DNSHostname}");
                        }
                        else
                        {
                            // Set Id on host object
                            entity.Id = existing.Id;

                            // Write existing firewall settings
                            if (existing.WindowsFirewallSettingId > 0)
                            {
                                entity.WindowsFirewallSettingId = existing.WindowsFirewallSettingId;
                                entity.WindowsFirewallSetting.Id = existing.WindowsFirewallSettingId;
                                context.Entry(existing.WindowsFirewallSetting).CurrentValues.SetValues(entity.WindowsFirewallSetting);
                            }
                            else
                            {
                                // Write new firewall settings
                                Models.WindowsFirewallSetting tmp = context.WindowsFirewallSetting.Add(entity.WindowsFirewallSetting);
                                context.SaveChanges();
                                entity.WindowsFirewallSettingId = tmp.Id;
                            }

                            // Write base properties
                            context.Entry(existing).CurrentValues.SetValues(entity);
                            context.Entry(existing).State = System.Data.Entity.EntityState.Modified;

                            // Write port numbers
                            if (entity.Port != null)
                            {
                                foreach (Models.Port entry in entity.Port)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.Port> ex = existing.Port.Where(ep => ep.PortNumber == entry.PortNumber);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.Port currentEntry = entity.Port[entity.Port.IndexOf(entry)];
                                        Models.Port oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.Port.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Write fileshares
                            if (entity.FileShare != null)
                            {

                                foreach (Models.FileShare entry in entity.FileShare)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.FileShare> ex = existing.FileShare.Where(ep => ep.Name == entry.Name);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.FileShare currentEntry = entity.FileShare[entity.FileShare.IndexOf(entry)];
                                        Models.FileShare oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.FileShare.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Write programs
                            if (entity.Program != null)
                            {
                                // Write Programs
                                foreach (Models.Program entry in entity.Program)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.Program> ex = existing.Program.Where(ep => ep.Name == entry.Name);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.Program currentEntry = entity.Program[entity.Program.IndexOf(entry)];
                                        Models.Program oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.Program.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Write roles
                            if (entity.Role != null)
                            {
                                // Write Roles
                                foreach (Models.Role entry in entity.Role)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.Role> ex = existing.Role.Where(ep => ep.Name == entry.Name);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.Role currentEntry = entity.Role[entity.Role.IndexOf(entry)];
                                        Models.Role oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.Role.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Write firewall rules
                            if (entity.WindowsFirewallRule != null)
                            {
                                // Write Firewall rules
                                foreach (Models.WindowsFirewallRule entry in entity.WindowsFirewallRule)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.WindowsFirewallRule> ex = existing.WindowsFirewallRule.Where(ep => ep.Name == entry.Name);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.WindowsFirewallRule currentEntry = entity.WindowsFirewallRule[entity.WindowsFirewallRule.IndexOf(entry)];
                                        Models.WindowsFirewallRule oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.WindowsFirewallRule.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Write TCP Connections
                            if (entity.TCPConnection != null)
                            {
                                // Write TCP Connection
                                foreach (Models.TCPConnection entry in entity.TCPConnection)
                                {
                                    // Set hostId
                                    entry.HostId = existing.Id;

                                    IEnumerable<Models.TCPConnection> ex = existing.TCPConnection.Where(ep =>
                                        ep.Direction == entry.Direction &&
                                        ep.RemoteIPAddress == entry.RemoteIPAddress &&
                                        ep.LocalPort == entry.LocalPort &&
                                        ep.RemotePort == entry.RemotePort);
                                    if (ex.Any())
                                    {
                                        // Copy all values to existing record
                                        Models.TCPConnection currentEntry = entity.TCPConnection[entity.TCPConnection.IndexOf(entry)];
                                        Models.TCPConnection oldEntry = ex.First();

                                        currentEntry.Id = oldEntry.Id;
                                        context.Entry(oldEntry).CurrentValues.SetValues(currentEntry);
                                        context.Entry(oldEntry).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create new record
                                        context.TCPConnection.Add(entry);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            // Remove old records, except for TCPConnections
                            IEnumerable<Models.Port> portsToRemove = existing.Port.Where(p => entity.Port.All(p2 => p2.PortNumber != p.PortNumber));
                            IEnumerable<Models.Role> rolesToRemove = existing.Role.Where(p => entity.Role.All(p2 => p2.Name != p.Name));
                            IEnumerable<Models.Program> programsToRemove = existing.Program.Where(p => entity.Program.All(p2 => p2.Name != p.Name));
                            IEnumerable<Models.FileShare> sharesToRemove = existing.FileShare.Where(p => entity.FileShare.All(p2 => p2.Name != p.Name));
                            IEnumerable<Models.WindowsFirewallRule> firewallRulesToRemove = existing.WindowsFirewallRule.Where(p => entity.WindowsFirewallRule.All(p2 => p2.Name != p.Name));

                            context.Port.RemoveRange(portsToRemove);
                            context.Role.RemoveRange(rolesToRemove);
                            context.Program.RemoveRange(programsToRemove);
                            context.FileShare.RemoveRange(sharesToRemove);
                            context.WindowsFirewallRule.RemoveRange(firewallRulesToRemove);

                            // Write unsynced changes
                            if (context.ChangeTracker.HasChanges())
                                context.SaveChanges();

                            //Console.WriteLine($"Updated: {entity.DNSHostname}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

            }

        }


        /// <summary>
        /// Adds a list of records to the database
        /// This methods assumes that all entities are new/unique
        /// </summary>
        /// <param name="entities"></param>
        public void AddHostRecords(List<Models.Host> entities)
        {
            using (DatabaseContext context = new DatabaseContext(dbLocation))
            {
                try
                {
                    context.Host.AddRange(entities);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Adds new query to the database
        /// </summary>
        /// <param name="query"></param>
        public void AddQuery(Models.Query query)
        {
            using (DatabaseContext context = new DatabaseContext(dbLocation))
            {
                try
                {
                    context.Query.Add(query);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// returns all queries from the database
        /// </summary>
        /// <param name="query"></param>
        public List<Models.Query> GetQueries()
        {
            List<Models.Query> list = new List<Models.Query>();

            using (DatabaseContext context = new DatabaseContext(dbLocation))
            {
                try
                {
                    list = context.Query.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return list;
        }

        /// <summary>
        /// Returns a list with all hosts from the database, based on the parameter if entity should be a server or not
        /// </summary>
        /// <param name="isServer"></param>
        /// <returns></returns>
        public List<Models.Host> GetHosts(bool isServer)
        {
            List<Models.Host> list = new List<Models.Host>();

            using (DatabaseContext context = new DatabaseContext(dbLocation))
            {
                try
                {
                    list = context.Host.Where(h => h.IsServerOS == isServer).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return list;
        }

        #endregion



        #region private methods

        /// <summary>
        /// Starts a new transaction on the database
        /// </summary>
        private void StartTransaction()
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            // Tweak the sqlite database to increase performance
            EnableDatabaseTweaks();

            transaction = conn.BeginTransaction();
            IsTransaction = true;
        }

        /// <summary>
        /// Commits transaction to the sqlite database
        /// </summary>
        private void EndTransaction()
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            transaction.Commit();
            IsTransaction = false;

            // Tweak the sqlite database to increase performance
            DisableDatabaseTweaks();
        }

        /// <summary>
        /// Tweak database to enhance performance. 
        /// Since the data is not mission critical, datacorruption 
        /// because of power outtage does not outweigh the performance increase. This means that some PRAGMA settings
        /// can be risky.
        /// </summary>
        private void EnableDatabaseTweaks()
        {
            // Keep journal logs in memory. 
            ExecuteNonQuery("PRAGMA JOURNAL_MODE=MEMORY;");

            // Use RAM as temp storage
            ExecuteNonQuery("PRAGMA TEMP_STORE=MEMORY;");

            // No synchronous journal rollback
            ExecuteNonQuery("PRAGMA SYNCHRONOUS=OFF;");

            // Set database in exclusive mode
            ExecuteNonQuery("PRAGMA LOCKING_MODE=EXCLUSIVE;");
        }

        /// <summary>
        /// Disables tweaks to enhance database performance
        /// </summary>
        private void DisableDatabaseTweaks()
        {

            ExecuteNonQuery("PRAGMA JOURNAL_MODE=DELETE;");
            ExecuteNonQuery("PRAGMA TEMP_STORE=DEFAULT;");
            ExecuteNonQuery("PRAGMA SYNCHRONOUS=FULL;");
            ExecuteNonQuery("PRAGMA LOCKING_MODE=NORMAL;");
        }

        /// <summary>
        /// Initializes the database
        /// </summary>
        private void Init()
        {
            bool isNew = false;

            // Check if dbLocation is a directory          
            if (Directory.Exists(dbLocation))
            {
                Util.Logger.LogDebug("Got directory as DB location");
                dbLocation = Path.Combine(dbLocation, Program.DB_FILENAME);
            }

            // Check if dbLocation is a file
            if (!File.Exists(dbLocation))
            {
                Util.Logger.LogDebug("DB does not exist: " + dbLocation);
                isNew = true;
            }

            connectionString = "Data Source=" + dbLocation;
            conn = new SQLiteConnection(connectionString);

            // Do we need to built a new structure?
            if (isNew || !hasDBStructure())
            {
                Util.Logger.LogDebug("Initializing database...");
                ExecuteNonQuery(Properties.Resources.CreateStructure);

                // Set flag to know that the database is fresh
                this.isNew = true;
            }

        }

        /// <summary>
        /// Run query against database
        /// </summary>
        /// <param name="query"></param>
        private void ExecuteNonQuery(string query)
        {
            SQLiteCommand sqlCommand = new SQLiteCommand();
            sqlCommand.Connection = conn;

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            if (IsTransaction)
                sqlCommand.Transaction = transaction;

            sqlCommand.CommandText = query;
            int result = sqlCommand.ExecuteNonQuery();

            if (sqlCommand != null)
                sqlCommand.Dispose();
        }

        /// <summary>
        /// Run query against database
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramNames"></param>
        /// <param name="paramValues"></param>
        private void ExecuteNonQuery(string query, string[] paramNames, object[] paramValues)
        {
            SQLiteCommand sqlCommand = new SQLiteCommand();
            sqlCommand.Connection = conn;

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            if (IsTransaction)
                sqlCommand.Transaction = transaction;

            sqlCommand.CommandText = query;
            for (int i = 0; i < paramNames.Length; i++)
                sqlCommand.Parameters.AddWithValue(paramNames[i], paramValues[i]);

            int result = sqlCommand.ExecuteNonQuery();

            if (sqlCommand != null)
                sqlCommand.Dispose();
        }

        /// <summary>
        /// Counts number of tables in database to determine if the database has been initialized before
        /// </summary>
        /// <returns></returns>
        private bool hasDBStructure()
        {
            int count = 0;
            bool result = false;
            string Query = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' " +
                           "AND name IN ('FileShare', 'Query', 'Host', 'Port', 'Program', 'Role', 'TCPConnection', 'WindowsFirewallRule', 'WindowsFirewallSetting');";

            if (conn.State != ConnectionState.Open)
                conn.Open();

            SQLiteCommand sqlComm = new SQLiteCommand(Query, conn);
            SQLiteDataReader reader = sqlComm.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    count = reader.GetInt32(0);
                    result = count == 9;
                }

            sqlComm.Dispose();
            return result;
        }

        #endregion

    }
}

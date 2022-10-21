using Antignis.Server.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Antignis.Server
{
    internal sealed class Program
    {
        #region Options

        /// <summary>
        /// Name of the database
        /// </summary>
        public const string DB_FILENAME = "antignis.db";

        /// <summary>
        /// Flag to enable debug logging
        /// </summary>
        internal static bool Debug = false;

        /// <summary>
        /// Flag to enable more verbosity logging
        /// </summary>
        internal static bool Verbose = false;

        /// <summary>
        /// LDAP client that interacts with Active Directory
        /// </summary>
        private static Core.ADDS.LDAP ldapClient = null;

        /// <summary>
        /// Database client that interacts with the database
        /// </summary>
        private static Core.SQL.Database dbClient = null;

        /// <summary>
        /// Location of the database
        /// </summary>
        private static string DatabaseLocation { get; set; } = $"{Environment.CurrentDirectory}";

        /// <summary>
        /// Flag to create a baseline for all Workstation in AD
        /// </summary>
        private static bool CreateWorkStationBaseline { get; set; }

        /// <summary>
        /// Flag to create a baseline for all servers in AD
        /// </summary>
        private static bool CreateServerBaseline { get; set; }
        /// <summary>
        /// Flag to create a test data set
        /// </summary>
        private static bool GenerateTestData { get; set; }

        /// <summary>
        /// Flag to import data from disk
        /// </summary>
        private static bool ImportFromDisk { get; set; }

        /// <summary>
        /// Flag to import from AD
        /// </summary>
        private static bool ImportFromAD { get; set; }

        /// <summary>
        /// Location where json files are stored
        /// </summary>
        private static string FilesDirectory { get; set; }

        /// <summary>
        /// Flag to configure a save location with correct ACL
        /// </summary>
        private static bool ConfigureSaveLocation { get; set; }

        /// <summary>
        /// Location of which ACL needs to be modified
        /// </summary>
        private static string SaveLocation { get; set; }
        #endregion

        private static void Main(string[] args)
        {
            ShowLogo();

            if (!ParseArguments(args))
            {
                Logger.Log("Press a key to exit...");
                Console.ReadKey();
                return;
            }


            try
            {
                // Stop running if any of the sanity checks fails
                if (!SanityChecks())
                {
                    Logger.Log("Press a key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Configures a directory with write-only permissions
                if (ConfigureSaveLocation)
                {
                    Core.Data.FS.CreateWriteOnlyDirectory(SaveLocation);
                    Logger.Log("Directory created and configured");
                    return;
                }

                if (ImportFromAD)
                {
                    // Ingest workstations from LDAP
                    Core.Util.Logger.LogDebug("Importing workstations from AD");
                    Startup.ImportWorkStationsFromAD(ldapClient, dbClient);
                    Logger.Log("Workstation imported");

                    // Ingest servers from LDAP
                    Core.Util.Logger.LogDebug("Importing servers from AD");
                    Startup.ImportServersFromAD(ldapClient, dbClient);
                    Logger.Log("Servers imported");

                    return;
                }

                // Import files from disk
                if (ImportFromDisk)
                {
                    Core.Util.Logger.LogDebug("Importing data from: " + FilesDirectory);
                    List<Core.Models.Host> hosts = ImportDataFromDisk(FilesDirectory);
                    dbClient.AddHostRecord(hosts);
                    Logger.Log("Files imported");
                    return;
                }

                // Generate test data?
                if (GenerateTestData)
                {
                    Core.Util.Logger.LogDebug("Generating testdata...");
                    GenerateAndImportTestData();
                    Logger.Log("Testdata created");
                    return;
                }

                // Check if we need to create a baseline for workstations 
                if (CreateWorkStationBaseline)
                {
                    // Create a baseline policy
                    Core.Util.Logger.LogDebug("Creating baseline for workstations...");
                    Startup.CreateFirewallBaseline(ldapClient, dbClient, false);
                    Logger.Log("Firewall baseline for workstations created");
                    return;
                }

                // Check if we need to create a baseline for workstations 
                if (CreateServerBaseline)
                {
                    // Create a baseline policy
                    Core.Util.Logger.LogDebug("Creating baseline for servers...");
                    Startup.CreateFirewallBaseline(ldapClient, dbClient, true);
                    Logger.Log("Firewall baseline for servers created");
                    return;
                }

                Core.Util.Logger.LogDebug("Starting GUI");
                Core.Data.Querier.DataQuerier q = new Core.Data.Querier.DataQuerier(dbClient, ldapClient);
                q.ShowDialog();

            }
            finally
            {
                if (ldapClient != null)
                    ldapClient.Dispose();
            }
        }

        /// <summary>
        /// Shows Antignis ASCIIart logo
        /// Source: https://www.asciiart.eu/animals/insects/ants
        /// </summary>
        private static void ShowLogo()
        {
            Console.WriteLine(@"       ,_     _,            _____          __  .__              .__        ");
            Console.WriteLine(@"         '._.'             /  _  \   _____/  |_|__| ____   ____ |__| ______");
            Console.WriteLine(@"    '-,   (_)   ,-'       /  /_\  \ /    \   __\  |/ ___\ /    \|  |/  ___/");
            Console.WriteLine(@"      '._ .:. _.'        /    |    \   |  \  | |  / /_/  >   |  \  |\___ \ ");
            Console.WriteLine(@"       _ '|Y|' _         \____|__  /___|  /__| |__\___  /|___|  /__/____  >");
            Console.WriteLine(@"     ,` `>\ /<` `,               \/     \/       /_____/      \/        \/");
            Console.WriteLine(@"    ` ,-`  I  `-, `");
            Console.WriteLine(@"      |   /=\   |        Rindert Kramer");
            Console.WriteLine(@"    ,-'   |=|   '-,      Hunt & Hackett, 2022");
            Console.WriteLine(@"          )-(");
            Console.WriteLine(@"          \_/");
            Console.WriteLine("");
         
        }

        /// <summary>
        /// Displays help
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("How to use:");
            Console.WriteLine("\t--settings:");
            Console.WriteLine("\t\tDisplays a form where settings can be viewed or modified");
            Console.WriteLine("\t--database:");
            Console.WriteLine("\t\tLocation of Antignis database. Defaults to startup directory");
            Console.WriteLine("\t--wsbaseline:");
            Console.WriteLine("\t\tCreates a baseline for workstations");
            Console.WriteLine("\t--srvbaseline:");
            Console.WriteLine("\t\tCreates a baseline for servers");
            Console.WriteLine("\t--generatetestdata:");
            Console.WriteLine("\t\tGenerates testdata to play with");
            Console.WriteLine("\t--importfiles:");
            Console.WriteLine("\t\tImport files from a location on disk. Specify location after flag:");
            Console.WriteLine("\t\t--importfiles C:\\tmp");
            Console.WriteLine("\t--configuresavelocation:");
            Console.WriteLine("\t\tCreates the specified folder and configures it such that users can only write to it. Specify location after flag:");
            Console.WriteLine("\t\t--configuresavelocation \\\\remoteUNC\\sharename\\Folder");
            Console.WriteLine("\t--importad:");
            Console.WriteLine("\t\tImport hosts from Active Directory");
            Console.WriteLine("\t--help:");
            Console.WriteLine("\t\tShows this message");
        }

        /// <summary>
        /// Performs the following checks:
        ///     - Provided database location
        ///     - Connect to AD
        ///     - If RSAT is installed
        ///     - Connect to database
        ///     - If all config data is present. Will guide user through the steps
        /// If one check fails, all fails
        /// </summary>
        private static bool SanityChecks()
        {
            bool result = false;

            // Check if user is a domain user
            if (!Core.ADDS.LDAP.IsUserJoinedToDomain())
            {
                Logger.Log("Antignis should be run with a domain joined user account.");
                return result;
            }

            // Check if host is joined to the domain
            if (!Core.ADDS.LDAP.IsComputerJoinedToDomain())
            {
                Logger.Log("Antignis should be run from a domain joined computer");
                return result;
            }


            // Connect to AD
            ldapClient = new Core.ADDS.LDAP(false);
            if (!ldapClient.IsConnected)
            {
                Logger.Log("Could not connect to Active Directory. Run the tool from a domain joined host and try again.");
                return result;
            }
            Logger.LogVerbose("[Main] Connected to Active Directory");

            // Check if RSAT is installed
            bool rsatInstalled = Startup.RSATInstalled();
            if (!rsatInstalled)
            {
                // Error message is thrown to the console in the RSATInstalled function
                return result;
            }
            Logger.LogVerbose("[Main] RSAT is installed.");

            // Check config
            if (!Startup.CheckData(ldapClient))
                return result;

            Logger.LogVerbose("[Main] App settings configured");

            // Connect the database
            dbClient = new Core.SQL.Database(DatabaseLocation);
            if (!dbClient.IsConnected)
            {
                Logger.Log("Could not create a connection to local database. Check path to database file");
                return result;
            }
            Logger.LogVerbose("[Main] Database initialized");

            // Ask user to import AD stuff if the database is new
            if (dbClient.isNew && !ImportFromAD)
            {
                Logger.Log("Do you want to ingest workstations and servers from Active Directory? (Y/n): ");
                string ans = Console.ReadLine();

                ImportFromAD = ans.ToLower() == "y";
            }

            result = true;
            return result;
        }

        /// <summary>
        /// Generates a test data set and imports it into the database. Optionally, the data is also exported to AD
        /// </summary>
        /// <param name="domain"></param>
        private static void GenerateAndImportTestData(string domain = "")
        {
            bool importAD = false;

            // Ask if computer objects should be created as well
            Console.Write("Do you want to create computer objects in AD as well? (Y/n): ");
            string ans = Console.ReadLine();

            // Ask for OU to store computer object is
            if (ans.ToLower() == "y")
            {
                importAD = true;
                if (string.IsNullOrEmpty(Properties.Settings.Default.ComputerOUForTest))
                {
                    Console.Write("What is the Organization Unit where the computer objects should be created? This starts with OU=: ");
                    ans = Console.ReadLine();

                    if (!ans.ToLower().StartsWith("ou="))
                    {
                        Logger.Log("Input should be a distinguishedName.\r\nThis article might be helpful:\r\n\thttps://support.xink.io/support/solutions/articles/1000246165-how-to-find-the-distinguishedname-of-an-ou-");
                        return;
                    }

                    Properties.Settings.Default.ComputerOUForTest = ans;
                    Properties.Settings.Default.Save();
                }
            }

            // Generate the testdata
            Logger.Log("Generating and importing testdata. This might take a few minutes...");
            List<Core.Models.Host> hosts = Core.Data.Testdata.Examples.GetExampleData(ldapClient.domainFQDN);
            dbClient.AddHostRecords(hosts);

            Logger.Log("Testdata has been generated and imported into the database.");

            if (importAD)
            {
                Logger.Log("Importing objects into AD...");
                ldapClient.CreateComputerObjects(hosts);
            }

            Logger.Log("Done");

        }

        /// <summary>
        /// Deserializes json files from disk and returns it as a list of host files
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private static List<Core.Models.Host> ImportDataFromDisk(string location)
        {
            List<Core.Models.Host> hosts = new List<Core.Models.Host>();

            // Read all json files in directory
            string[] files = Directory.GetFiles(location, "*.json");

            // Deserialize to host object and add to list
            foreach (string file in files)
            {
                try
                {
                    string content = File.ReadAllText(file);
                    hosts.Add(Jil.JSON.Deserialize<Core.Models.Host>(content));
                }
                catch
                {
                    Logger.Log("Could not parse file: " + file);
                }
                finally
                {
                    File.Delete(file);
                }
            }

            return hosts;
        }

        /// <summary>
        /// Parse commandline arguments
        /// </summary>
        /// <param name="args"></param>
        private static bool ParseArguments(string[] args)
        {

            bool result = true;

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i].ToLower();
                argument = argument.TrimStart('-', '/');

                // display settings
                if (argument == "settings")
                {
                    Core.Settings.GUI settings = new Core.Settings.GUI();
                    settings.ShowDialog();

                    settings.Dispose();
                    // stop running after changing the settings
                    return false;
                }

                // Location where to save json file
                if (argument == "database" || argument == "db")
                {
                    DatabaseLocation = args[i + 1];
                    DatabaseLocation = DatabaseLocation
                        .Replace("\"", null)
                        .Replace("'", null);
                    i++; continue;
                }

                // CreateWorkstationBaseline
                if (argument == "wsbaseline" || argument == "wb")
                {
                    CreateWorkStationBaseline = true;
                    continue;
                }

                //configuresavelocation
                if (argument == "configuresavelocation" || argument == "csl")
                {
                    ConfigureSaveLocation = true;
                    SaveLocation = args[i + 1];
                    SaveLocation = SaveLocation
                        .Replace("\"", null)
                        .Replace("'", null);
                    i++; continue;
                }


                // CreateServerBaseline
                if (argument == "srvbaseline" || argument == "sb")
                {
                    CreateServerBaseline = true;
                    continue;
                }

                // Generate Testdata
                if (argument == "generatetestdata" || argument == "testdata")
                {
                    GenerateTestData = true;
                    continue;
                }

                // FilesDirectory
                if (argument == "importfiles" || argument == "if")
                {
                    ImportFromDisk = true;
                    FilesDirectory = args[i + 1];
                    FilesDirectory = FilesDirectory
                        .Replace("\"", null)
                        .Replace("'", null);

                    i++; continue;
                }

                // IngestFromAD
                if (argument == "importad" || argument == "ia")
                {
                    ImportFromAD = true;
                    continue;
                }

                // Help
                if (argument == "h" || argument == "help" || argument == "?")
                {
                    DisplayHelp();
                    return false;
                }

                // Verbose
                if (argument == "verbose" || argument == "v")
                {
                    Verbose = true;
                    continue;
                }

                // Debug
                if (argument == "debug" || argument == "d")
                {
                    Debug = true;
                    continue;
                }

                // Unknown argument
                Logger.Log("Unknown argument: " + argument);
                return false;
            }

            // make sure all arguments make sense
            if (ImportFromDisk && string.IsNullOrEmpty(FilesDirectory))
            {
                Logger.Log("Please specify directory containing the files to be imported");
                return false;
            }

            // Check if files are available if importFromDisk is selected
            if (ImportFromDisk)
            {
                // Check if directory exists
                if (!Directory.Exists(FilesDirectory))
                {
                    Logger.Log($"Directory does not exist: " + FilesDirectory);
                    return false;
                }

                // Check if directory contains json files
                if (Directory.GetFiles(FilesDirectory, "*.json").Length == 0)
                {
                    Logger.Log($"Directory does not contain any input files: " + FilesDirectory);
                    return false;
                }
            }

            // Check if database is stored on UNC location. SQLite does not support this.
            if (DatabaseLocation.StartsWith(@"\\"))
            {
                Logger.Log("Database location on UNC network share is not supported. " +
                    "Either move Antignis to a local drive or specify the location on the harddrive where the database resides.");
                return false;
            }

            // Make sure only new directories are configured
            if (ConfigureSaveLocation)
            {
                if (Directory.Exists(SaveLocation))
                {
                    Logger.Log($" The directory at '{SaveLocation}' already exists. Please specify a new location");
                    return false;
                }

            }

            return result;
        }
    }
}

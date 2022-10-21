using Antignis.Client.Core.Net;
using System;

namespace Antignis.Client
{
    internal class Program
    {

        /// <summary>
        /// Flag to use Verbose logging
        /// </summary>
        public static bool Verbose = true;

        /// <summary>
        /// Location where jsonfile will be saved
        /// </summary>
        private static string SaveLocation = $"{Environment.CurrentDirectory}";

        /// <summary>
        /// Set to true to use a more extensive set of ports to scan.
        /// </summary>
        private static bool ExtensivePorts = false;

        /// <summary>
        /// Entrypoint of program.
        /// 
        /// This program collects the following data:
        ///     - Info about endpoint, such as DNSname, IP info, OS version
        ///     - Installed roles
        ///     - Exposed SMB shares
        ///     - List of ports that host is listening on
        ///     - List of ports and IPs that have connection established
        ///     - List of allowed firewall ports
        ///     - List of installed programs
        /// 
        /// Info is saved on a specifiable location in json format
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {

            // Parse arguments
            if (!ParseArgs(args))
            {
                //return;
            }

            // Lists and models
            Core.Models.Host host = new Core.Models.Host();

            try
            {
                // Retrieve network information about host
                host.IPAddress = Util.GetHostIPAddress();
                host.NetworkMask = Util.GetSubnetMaskForIP(host.IPAddress);
                host.DNSHostname = Util.GetFQDN();

                // Retrieve information about OS
                host.OperatingSystem = Core.Misc.GetOSVersion();
                host.IsServerOS = host.OperatingSystem.ToLower().Contains("server") ? true : false;

                // Retrieve information about exposed shares
                host.FileShare = Core.Protocols.WMI.ListShares();

                // Retrieve information about installed roles that need port 445 or 3389 to be open
                if (host.IsServerOS)
                    host.Role = Core.Protocols.WMI.ListServerRoles();

                // Scan the following ports against each host:  
                int[] RCEPorts = Util.GetRCEPorts(ExtensivePorts);

                // Get active TCP connections to RCE ports
                host.TCPConnection = Netstat.GetRemoteConnections(RCEPorts);

                // Get list of RCE ports that this host is listening on
                host.Port = Netstat.IsListeningOnPorts(RCEPorts);

                // Get a list of installed programs
                host.Program = Core.Misc.GetInstalledPrograms();

                // Fetch Windows Firewall settings
                host.WindowsFirewallSetting = WindowsFirewall.GetFirewallSettings(RCEPorts);
                host.WindowsFirewallRule = WindowsFirewall.GetFirewallRules(RCEPorts);

                // Serialize to json and save it to disk
                //var jsonObject = JsonSerializer.Serialize(self);
                string jsonObject = Jil.JSON.Serialize(host);
                bool fileSaved = Core.Misc.SaveFile(jsonObject, SaveLocation);

            }
            catch (Exception ex)
            {
                // Make sure that in worst case scenarios, the client does not display any unhandeld exceptions
                // write exception to file
                var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var logDirectory = System.IO.Path.Combine(appdataPath, "Antignis");
                var logPath = System.IO.Path.Combine(appdataPath, "ClientError.log");

                if (!System.IO.Directory.Exists(logDirectory))
                    System.IO.Directory.CreateDirectory(logDirectory);

                System.IO.File.WriteAllText(logPath, ex.Message);
            }


        }

        /// <summary>
        /// Parse commandline arguments
        /// </summary>
        /// <param name="args"></param>
        private static bool ParseArgs(string[] args)
        {

            bool result = true;

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i].ToLower();
                argument = argument.TrimStart('-', '/');

                // Location where to save json file
                if (argument == "savelocation" || argument == "sl")
                {
                    SaveLocation = args[i + 1];
                    i++; continue;
                }

                // Verbose
                if (argument == "verbose" || argument == "v")
                {
                    Verbose = true;
                    continue;
                }

                // Help
                if (argument == "h" || argument == "help" || argument == "?")
                {
                    ShowHelp();
                    return false;
                }

                // Unknown argument
                Console.WriteLine("Unknown argument: " + argument);
                return false;

            }

            return result;
        }
    
        /// <summary>
        /// Displays help on the console
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("\t--savelocation: <path>");
            Console.WriteLine("\t--help: shows this information");
        }
    }
}

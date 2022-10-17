using Antignis.Server.Core.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Antignis.Server.Core.Data.Testdata
{
    internal sealed class Generator
    {

        /// <summary>
        /// returns true if all addresses have been depleted
        /// </summary>
        /// <returns></returns>
        public bool Depleted() { return IPAddresses.Count <= 0; }

        // List to track generated hostnames
        private readonly List<string> Hostnames = new List<string>();

        /// <summary>
        /// List with WinFirewall block action types
        /// </summary>
        private readonly List<string> blockAction = new List<string>()
            {
                "NET_FW_ACTION_BLOCK",
                "NET_FW_ACTION_ALLOW"
            };

        /// <summary>
        /// List with Windows Desktop versions
        /// </summary>
        private readonly List<string> WindowsWorkstationVersions = new List<string>()
        {
            "Windows XP SP3",
            "Windows 7",
            "Windows 8.1",
            "Windows 10",
            "Windows 10 version 1511",
            "Windows 10 version 1809",
            "Windows 10 version 20H2",
            "Windows 11",
            "Windows 11 version 21H2",
            "Windows 11 version 22H2"
        };

        /// <summary>
        /// List with Windows Server versions
        /// </summary>
        private readonly List<string> WindowsServerVersions = new List<string>()
        {
            "Windows 2000",
            "Windows Server 2003 R2",
            "Windows Server 2008 R2",
            "Windows Server 2012",
            "Windows Server 2012 R2",
            "Windows Server 2016",
            "Windows Server 2019",
            "Windows Server, version 1909",
            "Windows Server, version 20H2",
            "Windows Server 2022"
        };

        /// <summary>
        /// List with Windows Server roles
        /// </summary>
        private readonly List<string> WindowsServerRoles = new List<string>()
        {
            "Active Directory Certificate Services",
            "Active Directory Domain Services",
            "Active Directory Federation Services",
            "Active Directory Lightweight Directory Services",
            "Active Directory Rights Management Services",
            "Device Health Attestation",
            "DHCP Server",
            "DNS Server",
            "File and Storage Services",
            "Host Guardian Service",
            "Hyper-V",
            "Print and Document Services",
            "Remote Access",
            "Remote Desktop Services",
            "Volume Activation Services",
            "Web Server IIS",
            "Windows Server Essentials Experience",
            "Windows Server Update Services"
        };

        /// <summary>
        /// List with RCE or latmov ports
        /// </summary>
        private readonly List<int> PortList = new List<int>(){
            21,
            22,
            23,
            135,
            137,
            445,
            3389,
            5985,
            5986
        };

        /// <summary>
        /// List with programs
        /// </summary>
        private readonly List<string> ProgramList = new List<string>()
        {
            "Visual Studio Community 2022",
            "Intel(R) HID Event Filter",
            "Battle.net",
            "KeePass Password Safe 2.49",
            "Microsoft Edge",
            "Microsoft Edge Update",
            "Microsoft Edge WebView2 Runtime",
            "Nmap 7.92",
            "Notepad++ (32-bit x86)",
            "Npcap",
            "Wireshark 3.6.1 64-bit",
            "YubiKey Manager",
            "Microsoft .NET Host FX Resolver - 6.0.1 (x86)",
            "Microsoft Visual C++ 2015-2019 Redistributable (x86) - 14.28.29913",
            "vs_communitysharedmsi",
            "Microsoft .NET Runtime - 6.0.9 (x86)",
            "icecap_collectionresourcesx64",
            "dch_setup",
            "Intel® Software Installer",
            "vs_clickoncesigntoolmsi",
            "Microsoft .NET Framework 4.7.2 Targeting Pack",
            "Microsoft .NET Runtime - 6.0.1 (x86)",
            "vs_minshellinteropsharedmsi",
            "vs_clickoncebootstrappermsi",
            "vs_FileTracker_Singleton",
            "icecap_collection_neutral",
            "Microsoft Windows Desktop Runtime - 6.0.9 (x86)",
            "TbtLegacyPlug",
            "Microsoft Windows Desktop Runtime - 6.0.1 (x86)",
            "Microsoft ASP.NET Core 6.0.9 - Shared Framework (x86)",
            "Microsoft Visual C++ 2019 X86 Additional Runtime - 14.28.29913",
            "ClickOnce Bootstrapper Package for Microsoft .NET Framework",
            "Realtek Card Reader",
            "vs_filehandler_x86",
            "Microsoft Visual C++ 2019 X86 Minimum Runtime - 14.28.29913",
            "Microsoft .NET SDK 6.0.401 (x64)",
            "Thunderbolt™ Software",
            "Microsoft Intune Management Extension",
            "Teams Machine-Wide Installer",
            "vs_minshellmsires",
            "icecap_collectionresources",
            "vs_minshellsharedmsi",
            "Intel(R) Chipset Device Software",
            "Microsoft TestPlatform SDK Local Feed",
            "Microsoft .NET Host FX Resolver - 6.0.9 (x86)",
            "vs_CoreEditorFonts",
            "Microsoft ASP.NET Core 6.0.9 Shared Framework (x86)",
            "Microsoft .NET Host - 6.0.9 (x86)",
            "Microsoft .NET Framework 4.8 SDK",
            "vcpp_crt.redist.clickonce",
            "Microsoft .NET Framework 4.8 Targeting Pack (ENU)",
            "vs_tipsmsi",
            "Microsoft Visual Studio Setup WMI Provider",
            "Microsoft .NET Framework 4.7.2 Targeting Pack (ENU)",
            "vs_BlendMsi",
            "Entity Framework 6.2.0 Tools  for Visual Studio 2022",
            "Microsoft .NET Framework 4.8 Targeting Pack",
            "Microsoft Visual Studio Setup Configuration",
            "VS Immersive Activate Helper",
            "vs_SQLClickOnceBootstrappermsi",
            "Microsoft .NET Framework Cumulative Intellisense Pack for Visual Studio (ENU)",
            "IntelliTraceProfilerProxy",
            "vs_filehandler_amd64",
            "Expresso",
            "vs_clickoncebootstrappermsires",
            "vs_communitymsires",
            "Intel® Integrated Sensor Solution",
            "Microsoft .NET SDK 6.0.109 (x64)",
            "vs_devenvsharedmsi",
            "Microsoft Visual C++ 2010  x86 Redistributable - 10.0.40219",
            "Microsoft ASP.NET Core 6.0.1 Shared Framework (x86)",
            "Microsoft Visual C++ 2015-2019 Redistributable (x64) - 14.29.30135",
            "PowerShell 7.2.6.0-x64"
        };

        /// <summary>
        /// Name of the DNS. will be used for host/ domainname generation
        /// </summary>
        private string DNSname { get; set; }

        /// <summary>
        /// List of all IPaddresses. Items will be removed
        /// </summary>
        private readonly List<string> IPAddresses = new List<string>();

        /// <summary>
        /// A copy of all IPAddresses to reference stuff
        /// </summary>
        private readonly List<string> IPAddressesCopy = new List<string>();

        /// <summary>
        /// Randomizer
        /// </summary>
        private readonly Random rnd = new Random();

        /// <summary>
        /// Bogus data randomizer
        /// </summary>
        private readonly Faker f = new Faker();

        public Generator()
        {

            // Generate domain name
            DNSname = $"{f.Internet.DomainWord()}.local";
            PrepareData();
        }

        public Generator(string domainName)
        {
            DNSname = domainName;
            PrepareData();
        }

        /// <summary>
        /// Generate the data needed for later
        /// </summary>
        private void PrepareData()
        {
            // We want to correlate IPs. Generate 5 subnets
            int subnetCount = 5;
            for (int i = 0; i < subnetCount; i++)
            {
                int subnet = rnd.Next(253);
                for (int n = 1; n < 253; n++)
                {
                    IPAddresses.Add($"192.168.{subnet}.{n}");
                }
            }

            // Make a copy
            IPAddressesCopy.AddRange(IPAddresses);
        }

        /// <summary>
        /// returns a valid windows version based if target is Workstation or Server
        /// </summary>
        /// <param name="isServer"></param>
        /// <returns></returns>
        private string GetWindowsVersion(bool isServer)
        {
            int index = rnd.Next(0, 9);
            return isServer ? WindowsServerVersions[index] : WindowsWorkstationVersions[index];
        }

        /// <summary>
        /// returns unique hostname
        /// </summary>
        /// <returns></returns>
        private string GetHostName()
        {
            string result = string.Empty;
            bool unique = false;

            do
            {
                result = f.Random.String2(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
                unique = Hostnames.Where(c => c == result).Count() <= 0;
            } while (!unique);

            Hostnames.Add(result);

            return result;
        }

        /// <summary>
        /// Returns a generated example
        /// </summary>
        /// <returns></returns>
        public Host GetExample()
        {
            // Get IP for this host and remove it from the list
            string ip = IPAddresses.FirstOrDefault();
            IPAddresses.Remove(ip);

            bool isServer = f.Random.Bool();
            string hostnamePrefix = isServer ? "SERVER" : "DESKTOP";

            Host h = new Host();
            h.IPAddress = ip;
            h.IsServerOS = isServer;
            h.DNSHostname = $"{hostnamePrefix}-{GetHostName()}.{DNSname}";
            h.NetworkMask = "255.255.255.0";
            h.OperatingSystem = GetWindowsVersion(isServer);

            // Add listening ports
            h.Port = GetExamplePorts();

            // Add file shares
            h.FileShare = GetExampleShares();

            // Add TCP Connections
            h.TCPConnection = GetExampleTCPConnections(ip);

            // Add firewall settings
            h.WindowsFirewallSetting = GetExampleFirewallSettings();

            // Add firewall rules
            h.WindowsFirewallRule = GetExampleFirewallRules();

            // Add programs
            h.Program = GetExamplePrograms();

            // Add roles
            h.Role = GetExampleRoles(isServer);

            return h;
        }

        /// <summary>
        /// returns a random amount of ports, of which the ports are listed in the PortList list
        /// </summary>
        /// <returns></returns>
        private List<Port> GetExamplePorts()
        {
            List<Port> ports = new List<Port>();
            int exposedPortsNumber = rnd.Next(PortList.Count - 1);

            for (int i = 0; i < exposedPortsNumber; i++)
            {
                bool unique = false;
                do
                {
                    int portnumber = PortList[rnd.Next(PortList.Count - 1)];
                    if (ports.Where(p => p.PortNumber == portnumber).Count() > 0)
                        continue;

                    ports.Add(new Port() { PortNumber = portnumber });
                    unique = true;

                } while (!unique);
            }

            return ports;
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows-server/administration/server-core/server-core-roles-and-services
        /// </summary>
        /// <returns></returns>
        private List<Role> GetExampleRoles(bool isServer)
        {
            List<Role> roles = new List<Role>();

            // Only Windows Server OS supported for now
            if (!isServer)
                return roles;

            int rolecount = rnd.Next(WindowsServerRoles.Count - 1);

            for (int i = 0; i < rolecount; i++)
            {
                bool unique = false;
                do
                {
                    string role = WindowsServerRoles[rnd.Next(WindowsServerRoles.Count - 1)];
                    if (roles.Where(r => r.Name == role).Count() > 0)
                        continue;

                    roles.Add(new Role() { Name = role });
                    unique = true;

                } while (!unique);
            }

            return roles;
        }

        /// <summary>
        /// Populates WindowsFirewallSettings class
        /// </summary>
        /// <returns></returns>
        private WindowsFirewallSetting GetExampleFirewallSettings()
        {
            WindowsFirewallSetting fwSetting = new WindowsFirewallSetting();

            // For testing purposes, this defaults to true and with default block action
            fwSetting.DomainProfileEnabled = true;
            fwSetting.DomainProfileDefaultBlockAction = "FW_BLOCK";

            fwSetting.PublicProfileEnabled = f.Random.Bool();
            fwSetting.PrivateProfileEnabled = f.Random.Bool();

            fwSetting.PublicProfileDefaultBlockAction = blockAction[rnd.Next(0, 1)];
            fwSetting.PrivateProfileDefaultBlockAction = blockAction[rnd.Next(0, 1)];

            return fwSetting;

        }

        /// <summary>
        /// Returns a list with Sharenames
        /// </summary>
        /// <returns></returns>
        private List<FileShare> GetExampleShares()
        {
            List<Core.Models.FileShare> fsList = new List<Core.Models.FileShare>();

            // Avoid creating shares for every resource
            if (!f.Random.Bool())
                return fsList;

            // No more than 4 shares per host
            int maxShareCount = 4;
            int sharenumber = rnd.Next(0, maxShareCount);

            for (int i = 0; i < sharenumber; i++)
            {
                fsList.Add(new FileShare()
                {
                    Name = f.Commerce.Department(1)
                });
            }

            return fsList;
        }

        /// <summary>
        /// returns a list with programs
        /// </summary>
        /// <returns></returns>
        private List<Core.Models.Program> GetExamplePrograms()
        {
            List<Core.Models.Program> programs = new List<Core.Models.Program>();

            int programcount = rnd.Next(ProgramList.Count - 1);

            for (int i = 0; i < programcount; i++)
            {
                bool unique = false;
                do
                {
                    string role = ProgramList[rnd.Next(ProgramList.Count - 1)];
                    if (programs.Where(r => r.Name == role).Count() > 0)
                        continue;

                    programs.Add(new Core.Models.Program() { Name = role });
                    unique = true;

                } while (!unique);
            }

            return programs;
        }

        /// <summary>
        /// returns a list with example firewall rules
        /// </summary>
        /// <returns></returns>
        private List<WindowsFirewallRule> GetExampleFirewallRules()
        {
            List<WindowsFirewallRule> fwRules = new List<WindowsFirewallRule>();

            bool createPubAndPrivate = f.Random.Bool();

            // Maximum 10 rules per host
            int numberOfRules = rnd.Next(0, 10);

            List<string> ruleProfile = new List<string>()
            {
                "Private",
                "Public"
            };

            // Domain profile
            for (int i = 0; i < numberOfRules; i++)
            {
                // roll dice to use random IP or a local IP
                string remoteAddress = f.Internet.Ip();
                if (f.Random.Bool())
                    remoteAddress = IPAddressesCopy[rnd.Next(IPAddressesCopy.Count - 1)];

                string ports = PortList[rnd.Next(PortList.Count - 1)].ToString();
                fwRules.Add(new WindowsFirewallRule()
                {
                    Profiles = "Domain",
                    Interfaces = "Ethernet0",
                    LocalPorts = ports,
                    RemoteAddresses = remoteAddress,
                    RuleEnabled = true,
                    Action = "FW_ALLOW",
                    Name = $"Allow {ports}"
                });

            }

            // public and private
            if (createPubAndPrivate)
            {
                numberOfRules = rnd.Next(0, 10);
                for (int i = 0; i < numberOfRules; i++)
                {
                    // roll dice to use random IP or a local IP
                    string remoteAddress = f.Internet.Ip();
                    if (f.Random.Bool())
                        remoteAddress = IPAddressesCopy[rnd.Next(IPAddressesCopy.Count - 1)];

                    string ports = PortList[rnd.Next(PortList.Count - 1)].ToString();
                    fwRules.Add(new WindowsFirewallRule()
                    {
                        Profiles = ruleProfile[rnd.Next(0, 1)],
                        Interfaces = "Ethernet0",
                        LocalPorts = ports,
                        RemoteAddresses = remoteAddress,
                        RuleEnabled = f.Random.Bool(),
                        Action = blockAction[rnd.Next(0, 1)],
                        Name = $"{blockAction[rnd.Next(0, 1)].Substring(3)} {ports}"
                    });

                }
            }


            return fwRules;

        }

        /// <summary>
        /// Generates a list with TCPConnections
        /// </summary>
        /// <param name="localIP"></param>
        /// <returns></returns>
        private List<TCPConnection> GetExampleTCPConnections(string localIP)
        {
            List<TCPConnection> tcpConnList = new List<TCPConnection>();

            // Do not create a connlist for every host
            if (!f.Random.Bool())
                return tcpConnList;

            // Maximum 5 rules per host
            int numberOfConnections = rnd.Next(0, 5);

            for (int i = 0; i < numberOfConnections; i++)
            {
                string remoteIp = f.Random.Bool() ? f.Internet.Ip() :
                    IPAddressesCopy[rnd.Next(0, IPAddressesCopy.Count - 1)];

                tcpConnList.Add(new TCPConnection()
                {
                    Direction = "Inbound",
                    LocalIPAddress = localIP,
                    RemoteIPAddress = remoteIp,
                    LocalPort = PortList[rnd.Next(0, PortList.Count - 1)],
                    RemotePort = f.Internet.Port()
                });
            }



            return tcpConnList;

        }



    }
}

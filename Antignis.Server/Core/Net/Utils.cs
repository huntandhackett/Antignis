using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Antignis.Server.Core.Net
{
    internal class Utils
    {

        /// <summary>
        /// Gets IP address for NIC that looks something like Ethernet, Ethernet0, WiFi, wi-fi, etc.
        /// </summary>
        /// <returns></returns>
        public static string GetHostIPAddress()
        {

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("^(?:ethernet|wi-?fi)\\d{0,3}$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string objResult = string.Empty;

            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface adapter in interfaces.Where(x => x.OperationalStatus == OperationalStatus.Up))
                {
                    if (r.IsMatch(adapter.Name))
                    {
                        IPInterfaceProperties props = adapter.GetIPProperties();
                        UnicastIPAddressInformation result = props.UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork);
                        if (result != null)
                        {

                            if (result.SuffixOrigin == SuffixOrigin.LinkLayerAddress)
                                continue;

                            objResult = result.Address.ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Move along
            }


            return objResult;
        }

        /// <summary>
        /// Retrieves subnetmask for IP address
        /// </summary>
        /// <param name="sIPAddress"></param>
        /// <returns></returns>
        internal static string GetSubnetMaskForIP(string sIPAddress)
        {
            string result = string.Empty;

            try
            {
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (sIPAddress.Equals(unicastIPAddressInformation.Address.ToString()))
                            {
                                IPAddress ipMask = unicastIPAddressInformation.IPv4Mask;
                                result = ipMask.ToString();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Moving along
            }

            return result;
        }

        /// <summary>
        /// Tests whether IP address is in a private range. 
        /// Localhost will result in a false
        /// </summary>
        /// <param name="testIp"></param>
        /// <returns></returns>
        internal static bool IsIPInternal(string ip)
        {
            if (ip == "::1")
                return false;

            byte[] ipaddress = IPAddress.Parse(ip).GetAddressBytes();
            switch (ipaddress[0])
            {
                case 10:
                    return true;
                case 127:
                    return false;
                case 172:
                    return ipaddress[1] >= 16 && ipaddress[1] < 32;
                case 192:
                    return ipaddress[1] == 168;
                default:
                    return false;
            }
        }

        internal static bool IsIPLocalHost(string ip)
        {
            return (ip == "::1" || ip == "127.0.0.1");
        }

        /// <summary>
        /// Returns an array of ports that can be used to execute code on the underlying computer.
        /// By default, this limits to WMI, SMB, WSMAN, telnet, Teamviewer, RDP and VNC. 
        /// Set the extenstive boolean to true to get a more extensive list of ports belonging to applications known for RCE-capabilities
        /// thx: https://twitter.com/ptswarm/status/1311310897592315905
        /// </summary>
        /// <returns></returns>
        internal static List<int> GetRCEPorts(bool extensive = false)
        {
            List<int> ports = new List<int>();

            // Java RMI
            if (extensive)
                ports.AddRange(new int[] { 1090, 1098, 1099, 4444, 11099, 47001, 47002, 10999 });

            // Weblogic
            if (extensive)
                ports.AddRange(new int[] { 7000, 7001, 7002, 7003, 7004, 8000, 8001, 8002, 8003, 9000, 9001, 9002, 9003, 9503, 7070, 7071 });

            // JDWP
            if (extensive)
                ports.AddRange(new int[] { 45000, 45001 });

            // JMX
            if (extensive)
                ports.AddRange(new int[] { 8686, 9012, 50500 });

            // GlassFish
            if (extensive)
                ports.AddRange(new int[] { 4848 });

            // jBoss
            if (extensive)
                ports.AddRange(new int[] { 11111, 4444, 4445 });

            // Cisco Smart Install
            if (extensive)
                ports.AddRange(new int[] { 4786 });

            // HP Data Protector
            if (extensive)
                ports.AddRange(new int[] { 5555, 5556 });

            // Teamviewer
            ports.Add(5938);

            // SMB
            ports.AddRange(new int[] { 139, 445 });

            // WMI
            ports.Add(135);

            // WinRM\WSMAN
            ports.AddRange(new int[] { 5985, 5986 });

            // Telnet
            ports.Add(23);

            // SSH
            ports.Add(22);

            // VNC
            ports.Add(5900);

            // RDP
            ports.Add(3389);

            return ports;
        }


    }
}

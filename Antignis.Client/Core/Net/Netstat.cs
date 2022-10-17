using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Antignis.Client.Core.Net
{
    internal class Netstat
    {

        public class TCPConnection
        {
            public enum flow
            {
                inbound,
                outbound
            }

            public flow direction { get; set; }
            public string localIpAddress { get; set; }
            public string remoteIpAddress { get; set; }

            public int localPort { get; set; }
            public int remotePort { get; set; }
        }

        /// <summary>
        /// Returns a list of TCPConnections between the local computer and remote endpoint.
        /// Only connection to a predefined port is included in the results
        /// </summary>
        /// <param name="filterOnPorts"></param>
        /// <returns></returns>
        internal static List<Core.Models.TCPConnection> GetRemoteConnections(int[] filterOnPorts)
        {

            List<Core.Models.TCPConnection> connections = new List<Core.Models.TCPConnection>();

            try
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpConnection in tcpConnections)
                {
                    // Skip if remoteEndpoint is localhost
                    if (Util.IsIPLocalHost(tcpConnection.RemoteEndPoint.Address.ToString()))
                        continue;

                    Core.Models.TCPConnection conn = new Core.Models.TCPConnection();
                    conn.LocalIPAddress = tcpConnection.LocalEndPoint.Address.ToString();
                    conn.LocalPort = tcpConnection.LocalEndPoint.Port;
                    conn.RemoteIPAddress = tcpConnection.RemoteEndPoint.Address.ToString();
                    conn.RemotePort = tcpConnection.RemoteEndPoint.Port;

                    // Fow now, check for established connections only
                    if (tcpConnection.State == TcpState.Established)
                    {
                        // Is remotePort in RCEPort array?
                        if (filterOnPorts.Contains(tcpConnection.RemoteEndPoint.Port))
                        {
                            conn.Direction = "Outbound";
                            connections.Add(conn);
                            continue;
                        }

                        // Is localPort in RCEPort array?
                        if (filterOnPorts.Contains(tcpConnection.LocalEndPoint.Port))
                        {
                            conn.Direction = "Inbound";
                            connections.Add(conn);
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Moving along
            }

            return connections;

        }

        /// <summary>
        /// Returns a list of ports that this host is listening on
        /// </summary>
        /// <param name="filterOnPorts"></param>
        /// <returns></returns>
        internal static List<Core.Models.Port> IsListeningOnPorts(int[] filterOnPorts)
        {
            List<Core.Models.Port> ports = new List<Core.Models.Port>();

            try
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                System.Net.IPEndPoint[] IPEndpoints = ipProperties.GetActiveTcpListeners();

                foreach (System.Net.IPEndPoint IPE in IPEndpoints)
                {
                    // Only add IPv4 for now
                    if (IPE.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        continue;

                    // Only add port if it is part of our predefined set of ports
                    if (filterOnPorts.Contains(IPE.Port) && ports.Where(c => c.PortNumber == IPE.Port).Count() == 0)
                        ports.Add(new Core.Models.Port() { PortNumber = IPE.Port });
                }
            }
            catch (Exception)
            {
                //Moving along
            }


            return ports;
        }
    }
}

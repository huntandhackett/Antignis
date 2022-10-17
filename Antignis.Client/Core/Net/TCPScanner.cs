using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Antignis.Client.Core.Net
{
    internal class TCPScanner
    {
        private class isTcpPortOpen
        {
            public TcpClient MainClient { get; set; }
            public bool tcpOpen { get; set; }
        }

        /// <summary>
        /// The list with IPs to be pingscanned
        /// </summary>
        private List<string> _ipList { get; set; }

        /// <summary>
        /// Default icmp timeout
        /// </summary>
        private readonly int timeout = 200;

        /// <summary>
        /// The list with IPs that responded to the pingscan
        /// </summary>
        private readonly List<Models.Neighbor> results = new List<Models.Neighbor> { };

        /// <summary>
        /// Array with ports to be scanned
        /// </summary>
        private readonly int[] _portList;

        /// <summary>
        /// Random obejct to manage lockstates across threads
        /// </summary>
        private static readonly object lockObj = new object();


        // This list contains every object that can be disposed
        private readonly List<object> objList = new List<object>();


        /// <summary>
        /// Constructor. Makes sure that all info is present
        /// </summary>
        public TCPScanner(List<string> IPList, int[] portList)
        {
            _ipList = IPList;
            _portList = portList;
        }

        public void Start()
        {
            // Start a task for every port on every host
            List<Task> tasks = new List<Task>();
            foreach (string ip in _ipList)
            {
                foreach (int port in _portList)
                {
                    TcpClient tcpClient = new TcpClient();
                    objList.Add(tcpClient);
                    Task task = StartTCPScanAsync(tcpClient, ip, port);
                    tasks.Add(task);
                }
            }

            Task.WaitAll(tasks.ToArray());

            // cleanup
            // Cleanup all ping methods
            foreach (IDisposable o in objList.OfType<IDisposable>())
            {
                if (o != null)
                    o.Dispose();
            }

        }

        private async Task StartTCPScanAsync(TcpClient tcpClient, string ip, int port)
        {
            try
            {
                isTcpPortOpen state = new isTcpPortOpen
                {
                    MainClient = tcpClient,
                    tcpOpen = true
                };

                IAsyncResult ar = tcpClient.BeginConnect(ip, port, AsyncCallback, state);
                state.tcpOpen = ar.AsyncWaitHandle.WaitOne(timeout, false);

                if (state.tcpOpen == false || tcpClient.Connected == false)
                {
                    return;
                }

                // We got a connection, add to the list
                lock (lockObj)
                {
                    // Check if results contains an Neighbor entry for our IP address
                    int ientry = results.Where(e => e.IPAddress == ip).Count();

                    if (ientry <= 0)
                    {
                        // Does not exist. Create new entry
                        results.Add(new Models.Neighbor { IPAddress = ip, PortsOpen = new List<int>() });
                    }

                    // Add port to entry
                    Models.Neighbor entry = results.Where(e => e.IPAddress == ip).First();
                    entry.PortsOpen.Add(port);
                }
            }
            catch (Exception)
            {
                // Don't handle exception, just continue
            }


        }


        public List<Models.Neighbor> GetData()
        {
            return results;
        }

        /// <summary>
        /// Handles logic for async callback from tcpclient
        /// </summary>
        /// <param name="asyncResult"></param>
        private static void AsyncCallback(IAsyncResult asyncResult)
        {
            if (asyncResult != null)
            {
                if (asyncResult.AsyncState != null)
                {
                    isTcpPortOpen state = (isTcpPortOpen)asyncResult.AsyncState;
                    TcpClient client = state.MainClient;

                    try
                    {
                        client.EndConnect(asyncResult);
                    }
                    catch
                    {
                        return;
                    }

                    if (client.Connected && state.tcpOpen)
                    {
                        return;
                    }

                    client.Close();

                }
            }
        }
    }
}

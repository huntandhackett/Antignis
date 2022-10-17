using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Antignis.Client.Core.Net
{
    // thx: https://www.justinmklam.com/posts/2018/02/ping-sweeper/
    public class PingScanner
    {
        /// <summary>
        /// The list with IPs to be pingscanned
        /// </summary>
        private List<string> _ipList { get; set; }

        /// <summary>
        /// Default icmp timeout
        /// </summary>
        private readonly int timeout = 100;

        /// <summary>
        /// The list with IPs that responded to the pingscan
        /// </summary>
        private readonly List<string> result = new List<string> { };

        /// <summary>
        /// Random obejct to manage lockstates across threads
        /// </summary>
        private static readonly object lockObj = new object();

        /// <summary>
        /// Constructor. Makes sure that all info is present
        /// </summary>
        public PingScanner(List<string> IPList)
        {
            _ipList = IPList;
        }

        // This list contains every object that can be disposed
        private readonly List<object> objList = new List<object>();

        /// <summary>
        /// Starts the ping scan
        /// </summary>
        public void Start()
        {
            List<Task> tasks = new List<Task>();
            foreach (string ip in _ipList)
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                objList.Add(ping);
                Task task = StartPingScanAsync(ping, ip);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            // Cleanup all ping methods
            foreach (IDisposable o in objList.OfType<IDisposable>())
            {
                if (o != null)
                    o.Dispose();
            }
        }

        /// <summary>
        /// returns the result
        /// </summary>
        /// <returns></returns>
        public List<string> GetData()
        {
            return result;
        }

        private async Task StartPingScanAsync(System.Net.NetworkInformation.Ping pingObj, string ip)
        {
            try
            {
                System.Net.NetworkInformation.PingReply reply = await pingObj.SendPingAsync(ip, timeout);

                // Add ip to list when a reply has been received
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    lock (lockObj)
                    {
                        result.Add(ip);
                    }
                }
            }
            catch
            {
                // Don;t handle exception, just continue
            }
        }
    }
}

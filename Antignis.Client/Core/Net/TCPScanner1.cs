using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Antignis.Client.Core.Net
{

    // thx: https://stackoverflow.com/a/22689821


    internal class TCPScanner1
    {

        /// <summary>
        /// Number of threads to work with
        /// </summary>
        private static int ThreadCount = 30;

        /// <summary>
        /// Ports to scan
        /// </summary>
        private static int[] Ports;

        /// <summary>
        /// Input queue
        /// </summary>
        private static readonly BlockingCollection<string> inputQueue = new BlockingCollection<string>();

        /// <summary>
        /// Output queue
        /// </summary>
        private static readonly BlockingCollection<Models.Neighbor> outputQueue = new BlockingCollection<Models.Neighbor>();

        /// <summary>
        /// List that is returned to caller
        /// </summary>
        private static readonly List<Models.Neighbor> OutputList = new List<Models.Neighbor>();

        /// <summary>
        /// Starts logic to scan the network for hosts that listen on given array of ports
        /// </summary>
        /// <param name="IPsInNetwork">list with IPs to scan</param>
        /// <param name="PortsToScan">array with ports to scan the IPs</param>
        /// <param name="threads">The amount of threads to use</param>
        /// <param name="pingscan">Flag to do a pingscan first</param>
        /// <returns></returns>
        public static List<Models.Neighbor> ScanNetwork(List<string> IPsInNetwork, int[] PortsToScan, int threads, bool pingscan)
        {

            Ports = PortsToScan;
            ThreadCount = threads;

            Task[] workers = new Task[ThreadCount];
            Task.Factory.StartNew(consumer);

            // Start workers
            for (int i = 0; i < ThreadCount; i++)
            {
                int workerID = i;
                Task task = new Task(() => worker(workerID));
                workers[i] = task;
                task.Start();
            }

            // Add IPs to the inputqueue
            foreach (string ip in IPsInNetwork)
            {
                inputQueue.Add(ip);
                Thread.Sleep(50);
            }
            inputQueue.CompleteAdding();

            // Wait for all workers to finish
            Task.WaitAll(workers);
            outputQueue.CompleteAdding();

            return OutputList;
        }

        private static void consumer()
        {
            foreach (Models.Neighbor item in outputQueue.GetConsumingEnumerable())
            {
                OutputList.Add(item);
                Thread.Sleep(25);
            }
        }

        private static void worker(int workerID)
        {
            foreach (string ip in inputQueue.GetConsumingEnumerable())
            {
                Misc.WriteGood(string.Format("Thread {0} scanning IP: {1}", workerID + 1, ip));

                Models.Neighbor nb = scan(ip);

                // if nb has ports open, add it to outputqueue
                if (nb.PortsOpen.Count() > 0)
                    outputQueue.Add(nb);
            }
        }

        private static Models.Neighbor scan(string ip)
        {
            Models.Neighbor nb = new Models.Neighbor();
            nb.PortsOpen = new List<int>();
            nb.IPAddress = ip;

            int timeout = 1000;

            foreach (int p in Ports)
            {
                if (Portscanner.Connect(ip, p, timeout))
                    nb.PortsOpen.Add(p);
            }

            return nb;
        }
    }
}

using System;
using System.Net.Sockets;

namespace Antignis.Client.Core.Net
{
    internal class Portscanner
    {

        private class isTcpPortOpen
        {
            public TcpClient MainClient { get; set; }
            public bool tcpOpen { get; set; }
        }

        /// <summary>
        /// Handles logic for async callback from tcpclient
        /// </summary>
        /// <param name="asyncResult"></param>
        private static void AsyncCallback(IAsyncResult asyncResult)
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

        /// <summary>
        /// Connect to remote host and port with timeout
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool Connect(string hostName, int port, int timeout)
        {
            bool result = true;
            TcpClient newClient = new TcpClient();
            isTcpPortOpen state = new isTcpPortOpen
            {
                MainClient = newClient,
                tcpOpen = true
            };

            IAsyncResult ar = newClient.BeginConnect(hostName, port, AsyncCallback, state);
            state.tcpOpen = ar.AsyncWaitHandle.WaitOne(timeout, false);

            if (state.tcpOpen == false || newClient.Connected == false)
            {
                result = false;
            }
            return result;
        }


    }
}

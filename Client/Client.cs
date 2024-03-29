﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SUS.Client
{
    public static class AsynchronousClient
    {
        // The port number for the remote device.  
        private const int Port = 8411;

        // ManualResetEvent instances signal completion.  
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        /// <summary>
        ///     Sets the Client up to begin interacting with the server.
        /// </summary>
        public static Socket StartClient()
        {
            // Connect to a remote device.  
            // Establish the remote endpoint for the socket.  
            var ipHostInfo = Dns.GetHostEntry("localhost");
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEp = new IPEndPoint(ipAddress, Port);

            // Create a TCP/IP socket.  
            var client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("[ Client Started ]");

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEp, ConnectCallback, client);
            ConnectDone.WaitOne();

            // CORE of the server. Handles actions made by client and information from the Server.
            return client;
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket) ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine($"Socket connected to {client.RemoteEndPoint}.");

                // Signal that the connection has been made.  
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
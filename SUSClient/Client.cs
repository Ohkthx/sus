using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Utility;

// State object for receiving data from remote device.  
namespace SUSClient.Client
{
   public class AsynchronousClient
    {
        // The port number for the remote device.  
        private const int port = 8411;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);

        public static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("[ Client Started ]");

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                Program.ServerHandler(ref client);

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
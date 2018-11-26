using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Utilities
{
    [Serializable]
    public enum RequestTypes
    {
        OK,
        Error,
        Authenticate,
        GameState,
        Mobile,
        LocalMobiles,
        MobileAction,
        MobileMove,
        Node,
        Player,
        Resurrection,
        SocketKill
    }

    /// <summary>
    ///     Holds various functions to perform actions.
    /// </summary>
    public static class Utility
    {
        public static void ConsoleNotify(string msg)
        {
            ConsoleColor cc = Console.ForegroundColor;          // Save the console's color.
            Console.ForegroundColor = ConsoleColor.DarkRed;     // Set the color to Dark Red.
            Console.WriteLine($" !! {msg}");
            Console.ForegroundColor = cc;                       // Reset the color to the default.
        }

        public static double RandomMinMax(double min, double max)
        {
            if (min > max)
            {
                double copy = min;
                min = max;
                max = copy;
            }
            else if (min == max)
                return min;

            return min + (RandomImpl.NextDouble() * (max - min));
        }

        public static int RandomMinMax(int min, int max)
        {
            if (min > max)
            {
                int copy = min;
                min = max;
                max = copy;
            }
            else if (min == max)
                return min;

            return min + RandomImpl.Next((max - min) + 1);
        }

        public static int Random(int from, int count)
        {
            if (count == 0)
                return from;
            else if (count > 0)
                return from + RandomImpl.Next(count);
            else
                return from - RandomImpl.Next(-count);
        }

        public static int Random(int count)
        {
            return RandomImpl.Next(count);
        }

        public static double RandomDouble()
        {
            return RandomImpl.NextDouble();
        }

        public static int RandomInt()
        {
            return RandomImpl.Next();
        }
    }

    /// <summary>
    ///     Prepares and deciphers data transferred over the network.
    /// </summary>
    public static class Network
    { 
        private const int HeaderSize = sizeof(long);    // A constant that stores length that prefixes the byte array.

        /// <summary>
        ///     Takes an object and converts it to a byte array prefixing its size.
        /// </summary>
        /// <param name="obj">Object to be converted.</param>
        /// <returns>Byte array containing the object.</returns>
        public static byte[] Serialize(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, obj);

                byte[] message = new byte[HeaderSize + memoryStream.Length];
                byte[] header = BitConverter.GetBytes(memoryStream.Length);
                byte[] body = memoryStream.ToArray();
                Array.Copy(header, 0, message, 0, header.Length);
                Array.Copy(body, 0, message, header.Length, body.Length);

                return message;
            }
        }

        /// <summary>
        ///     Takes a byte array and converts to to an object.
        /// </summary>
        /// <param name="message">Byte array to process.</param>
        /// <returns>An object to cast.</returns>
        public static Object Deserialize(byte[] message)
        {
            Object obj = new Object();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(message);

            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

            obj = bf.Deserialize(memoryStream);
            memoryStream.Close();

            return obj;
        }
    }
 
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Check if we have got the read size.
        public bool haveSize = false;
        // Stores the extracted object size.
        public long ObjectSize = -1;
        // Client  socket.  
        public Socket workSocket = null;
        public const int HeaderSize = sizeof(long);
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data total.
        public byte[] Value = null;

        // Extracts the size of the object.
        public bool ExtractSize(byte[] array, int size)
        {
            // Return early if it's already been performed. This should be called before attempting this.
            if (haveSize)
                return true;
            else if (ObjectSize >= 0)
                return true;
            else if (size < HeaderSize)
                return true;

            // Attempt to get first sizeof(long) bytes from buffer.
            ObjectSize = BitConverter.ToInt64(array, 0);

            // Reassign new buffer with trimmed header.
            var tBuffer = new byte[size - HeaderSize];
            Array.Copy(this.buffer, HeaderSize, tBuffer, 0, size - HeaderSize);

            // Add to our current value.
            this.Add(tBuffer, tBuffer.Length);

            haveSize = true;
            return false;
        }

        // Add to the current Value.
        public void Add(byte[] array, int size)
        {
            int iLength = 0;
            if (this.Value != null && this.Value.Length > 0)
                iLength = this.Value.Length;

            byte[] newValue = new byte[size + iLength];

            int offset = 0;
            if (this.Value != null && this.Value.Length > 0)
            {
                Array.Copy(this.Value, 0, newValue, 0, this.Value.Length);
                offset = this.Value.Length;
            }

            Array.Copy(array, 0, newValue, offset, size);

            this.Value = newValue;
        }
    }

    [Serializable]
    public sealed class Authenticate
    {
        public ulong ID { get; private set; }
        public Player Account = null;

        #region Constructors
        public Authenticate(ulong ID) : this(ID, null) { }
        public Authenticate(ulong ID, Player account)
        {
            this.ID = ID;
            this.Account = account;
        }
        #endregion

        public byte[] ToByte()
        {
            return Network.Serialize(this);
        }
    }

    [Serializable]
    public sealed class Request
    {
        public RequestTypes Type { get; private set; }  // Type of Request being made.
        public Object Value = null;                     // Object to be casted based on Type (RequestTypes.)

        // Creates an instance of a Request based on supplied Type and Object.
        public Request(RequestTypes type, Object obj)
        {
            this.Type = type;
            this.Value = obj;
        }

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte() { return Network.Serialize(this); }
    }

    [Serializable]
    public sealed class SocketKill
    {
        public bool killme { get; private set; }
        public Serial UserID { get; private set; }
        public SocketKill(Serial id, bool kill = true)
        {
            this.UserID = id;
            this.killme = kill;
        }

        public byte[] ToByte() { return Network.Serialize(this); }
    }

    public class SocketHandler
    {
        private bool DEBUG;
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent readDone = new ManualResetEvent(false);
        private Object response = null;
        private Socket socket = null;
        private Types type = Types.None;

        public enum Types { None = 0, Server = 1, Client = 2 }

        /// <summary>
        ///     Creates an instance of a SocketHandler with the information provided.
        /// </summary>
        public SocketHandler(Socket socket, Types type, bool debug = false)
        {
            this.type = type;
            this.socket = socket;
            this.DEBUG = debug;
        }

        private void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = socket;

                // Begin receiving the data from the remote device.  
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                readDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // Extract our ObjectSize if we haven't already.
                    if (state.ExtractSize(state.buffer, bytesRead))
                        // There  might be more data, so store the data received so far.
                        state.Add(state.buffer, bytesRead);

                    if (state.Value.Length != state.ObjectSize)
                    {
                        // Get the rest of the data.  
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    } 
                    else
                    {
                        if (this.DEBUG)
                            Console.WriteLine($" => {state.Value.Length + sizeof(long)} bytes read from {Enum.GetName(typeof(Types), this.type)}.");

                        this.response = Network.Deserialize(state.Value);

                        readDone.Set();
                    }
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.Value.Length == state.ObjectSize)
                    {
                        if (DEBUG)
                            Console.WriteLine($" => {state.Value.Length + sizeof(long)} read from {Enum.GetName(typeof(Types), this.type)}.");

                        this.response = Network.Deserialize(state.Value);
                    }

                    // Signal that all bytes have been received.  
                    readDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException)
            {
                Kill();
                return;
            }

            if (bytesRead > 0)
            {
                // Extract our ObjectSize if we haven't already.
                if (state.ExtractSize(state.buffer, bytesRead))
                    // There  might be more data, so store the data received so far.
                    state.Add(state.buffer, bytesRead);

                if (state.Value.Length == state.ObjectSize)
                {
                    if (this.DEBUG)
                        Console.WriteLine($" => {state.Value.Length + sizeof(long)} bytes read from {Enum.GetName(typeof(Types), this.type)}.");

                    this.response = Network.Deserialize(state.Value);
                    readDone.Set();
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ServerReadCallback), state);
                }

;
            }
        }

        private void ServerSendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                if (this.DEBUG)
                    Console.WriteLine($" <= {bytesSent} bytes sent to {Enum.GetName(typeof(Types), this.type)}.");

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(byte[] data)
        {
            // Begin sending the data to the remote device.
            try
            {
                socket.BeginSend(data, 0, data.Length, 0,
                        new AsyncCallback(SendCallback), socket);
            }
            catch (SocketException)
            {
                Kill();
                return;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                if (this.DEBUG)
                    Console.WriteLine($" <= {bytesSent} bytes sent to {Enum.GetName(typeof(Types), this.type)}.");

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public Object Communicate(byte[] data)
        {
            this.Send(data);
            sendDone.WaitOne();

            this.Receive();
            readDone.WaitOne();

            sendDone.Reset();
            readDone.Reset();

            var obj = this.response;
            this.response = null;

            return obj;
        }

        public Object FromServer()
        {
            this.Receive();
            readDone.WaitOne();
            readDone.Reset();

            var obj = this.response;
            this.response = null;

            return obj;
        }

        public void ToServer(byte[] data)
        {
            this.Send(data);
            sendDone.WaitOne();
            sendDone.Reset();
        }

        public Object FromClient()
        {
            StateObject state = new StateObject();
            state.workSocket = socket;
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ServerReadCallback), state);
            //this.Receive();
            readDone.WaitOne();

            var obj = this.response;
            this.response = null;

            readDone.Reset();

            return obj;
        }

        public void ToClient(byte[] data)
        {
            // Begin sending the data to the remote device.  
            socket.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(ServerSendCallback), socket);
            sendDone.WaitOne();

            sendDone.Reset();
        }

        public void Kill()
        {
            this.Send(new SocketKill(null, true).ToByte());
            sendDone.WaitOne();
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
            sendDone.Reset();
        }
    }

    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }
}

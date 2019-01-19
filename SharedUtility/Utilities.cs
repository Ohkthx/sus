using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SUS.Shared.Packets;

namespace SUS.Shared
{
    /// <summary>
    ///     Holds various functions to perform actions.
    /// </summary>
    public static class Utility
    {
        private static readonly object NotifyLock = new object();

        public static IEnumerable<T> EnumToIEnumerable<T>(Enum mask, bool powerOf2 = false)
        {   // Thanks to [stackoverflow.com/users/1612975].
            if (!typeof(T).IsEnum)
            {
                return new List<T>();
            }

            // Anonymous function that determines if it needs to validate if it is a power of two, if so- it does.
            bool PowerCheck(int v) => !powerOf2 || (v & (v - 1)) == 0;

            return Enum.GetValues(typeof(T))
                       .Cast<Enum>()
                       .Where(x => mask.HasFlag(x) && PowerCheck(Convert.ToInt32(x)))
                       .Cast<T>()
                       .Skip(1);
        }

        public static void ConsoleNotify(string msg)
        {
            lock (NotifyLock)
            {
                ConsoleColor cc = Console.ForegroundColor;          // Save the console's color.
                Console.ForegroundColor = ConsoleColor.DarkRed;     // Set the color to Dark Red.
                Console.WriteLine($" !! {msg}");
                Console.ForegroundColor = cc;                       // Reset the color to the default.
            }
        }

        public static double RandomMinMax(double min, double max)
        {
            if (min > max)
            {
                var copy = min;
                min = max;
                max = copy;
            }
            else if (min == max)
            {
                return min;
            }

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
            {
                return min;
            }

            return min + RandomImpl.Next((max - min) + 1);
        }

        public static int Random(int from, int count)
        {
            if (count == 0)
            {
                return from;
            }

            if (count > 0)
            {
                return from + RandomImpl.Next(count);
            }

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
            using (MemoryStream memoryStream = new MemoryStream())
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
        public static object Deserialize(byte[] message)
        {
            var bf = new BinaryFormatter();
            var memoryStream = new MemoryStream(message);

            bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

            var obj = bf.Deserialize(memoryStream);
            memoryStream.Close();

            return obj;
        }
    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Check if we have got the read size.
        private bool m_HaveSize;
        // Stores the extracted object size.
        public long ObjectSize = -1;
        // Client  socket.  
        public Socket workSocket;
        private const int HeaderSize = sizeof(long);
        public const int BufferSize = 1024;

        // Receive buffer.  
        public readonly byte[] buffer = new byte[BufferSize];
        // Received data total.
        public byte[] Value;

        // Extracts the size of the object.
        public bool ExtractSize(byte[] array, int size)
        {
            // Return early if it's already been performed. This should be called before attempting this.
            if (m_HaveSize)
            {
                return true;
            }

            if (ObjectSize >= 0)
            {
                return true;
            }

            if (size < HeaderSize)
            {
                return true;
            }

            // Attempt to get first sizeof(long) bytes from buffer.
            ObjectSize = BitConverter.ToInt64(array, 0);

            // Reassign new buffer with trimmed header.
            byte[] tBuffer = new byte[size - HeaderSize];
            Array.Copy(buffer, HeaderSize, tBuffer, 0, size - HeaderSize);

            // Add to our current value.
            Add(tBuffer, tBuffer.Length);

            m_HaveSize = true;
            return false;
        }

        // Add to the current Value.
        public void Add(byte[] array, int size)
        {
            int iLength = 0;
            if (Value != null && Value.Length > 0)
            {
                iLength = Value.Length;
            }

            byte[] newValue = new byte[size + iLength];

            int offset = 0;
            if (Value != null && Value.Length > 0)
            {
                Array.Copy(Value, 0, newValue, 0, Value.Length);
                offset = Value.Length;
            }

            Array.Copy(array, 0, newValue, offset, size);

            Value = newValue;
        }
    }

    public class SocketHandler
    {
        private readonly bool m_Debug;
        private readonly ManualResetEvent m_SendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent m_ReadDone = new ManualResetEvent(false);
        private object m_Response;
        private readonly Socket m_Socket;
        private readonly Types m_Type;

        public enum Types { Server = 1, Client = 2 }

        /// <summary>
        ///     Creates an instance of a SocketHandler with the information provided.
        /// </summary>
        public SocketHandler(Socket socket, Types type, bool debug = false)
        {
            m_Type = type;
            m_Socket = socket;
            m_Debug = debug;
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead;
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
                {
                    // There  might be more data, so store the data received so far.
                    state.Add(state.buffer, bytesRead);
                }

                if (state.Value.Length == state.ObjectSize)
                {
                    if (m_Debug)
                    {
                        Console.WriteLine($" => {state.Value.Length + sizeof(long)} bytes read from {Enum.GetName(typeof(Types), m_Type)}.");
                    }

                    m_Response = Network.Deserialize(state.Value);
                    m_ReadDone.Set();
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ServerReadCallback, state);
                }
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

                if (m_Debug)
                {
                    Console.WriteLine($" <= {bytesSent} bytes sent to {Enum.GetName(typeof(Types), m_Type)}.");
                }

                m_SendDone.Set();
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
                m_Socket.BeginSend(data, 0, data.Length, 0,
                        SendCallback, m_Socket);
            }
            catch (SocketException)
            {
                Kill();
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

                if (m_Debug)
                {
                    Console.WriteLine($" <= {bytesSent} bytes sent to {Enum.GetName(typeof(Types), m_Type)}.");
                }

                m_SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ToServer(byte[] data)
        {
            Send(data);
            m_SendDone.WaitOne();
            m_SendDone.Reset();
        }

        public object FromClient()
        {
            StateObject state = new StateObject
            {
                workSocket = m_Socket
            };
            m_Socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                ServerReadCallback, state);
            //this.Receive();
            m_ReadDone.WaitOne();

            object obj = m_Response;
            m_Response = null;

            m_ReadDone.Reset();

            return obj;
        }

        public void ToClient(byte[] data)
        {
            // Begin sending the data to the remote device.  
            m_Socket.BeginSend(data, 0, data.Length, 0,
                ServerSendCallback, m_Socket);
            m_SendDone.WaitOne();

            m_SendDone.Reset();
        }

        public void Kill()
        {
            Send(new SocketKillPacket(0).ToByte());
            m_SendDone.WaitOne();
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
            m_SendDone.Reset();
        }
    }

    internal sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

            var currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            var typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");

            return typeToDeserialize;
        }
    }
}

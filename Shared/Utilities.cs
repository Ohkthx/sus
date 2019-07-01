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
        private static int _badLogAttempts; // Counts the bad attempts made.
        private static readonly object NotifyLock = new object();

        /// <summary>
        ///     Get an integer from the console in regards to a list of options.
        /// </summary>
        /// <param name="maxSize">Size of the list.</param>
        /// <param name="zeroIsNone">If '0' in the list is a "None" option.</param>
        /// <returns>Integer selected.</returns>
        public static int ReadInt(int maxSize, bool zeroIsNone = false)
        {
            // Edits the maxsize in regards if zero is set as "none"
            maxSize = zeroIsNone ? maxSize - 1 : maxSize;

            int opt;
            do
            {
                Console.Write(" Selection: ");
                if (!int.TryParse(Console.ReadLine(), out opt))
                    continue;

                if (zeroIsNone && opt >= 0 && opt <= maxSize)
                    break;

                if (opt > 0 && opt <= maxSize)
                    break;
            } while (true);

            return opt;
        }

        /// <summary>
        ///     Attempt to convert user input to a desired Enum.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum.</typeparam>
        /// <returns>Enum selected.</returns>
        public static TEnum ReadEnum<TEnum>()
            where TEnum : struct
        {
            TEnum opt;
            do
            {
                Console.Write(" Selection: ");
            } while (!Enum.TryParse(Console.ReadLine(), true, out opt));

            return opt;
        }

        /// <summary>
        ///     Take an enum with a mask (bits set) and converts it to an IEnumerable.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum.</typeparam>
        /// <param name="mask">Set flags.</param>
        /// <param name="powerOf2">Does the Enum respect bit flagging?</param>
        /// <returns>IEnumerable that can be iterated.</returns>
        public static IEnumerable<TEnum> EnumToIEnumerable<TEnum>(Enum mask, bool powerOf2 = false)
        {
            // Thanks to [stackoverflow.com/users/1612975].
            if (!typeof(TEnum).IsEnum)
                return new List<TEnum>();

            // Anonymous function that determines if it needs to validate if it is a power of two, if so- it does.
            bool PowerCheck(int v)
            {
                return !powerOf2 || (v & (v - 1)) == 0;
            }

            return Enum.GetValues(typeof(TEnum))
                .Cast<Enum>()
                .Where(x => mask.HasFlag(x) && PowerCheck(Convert.ToInt32(x)))
                .Cast<TEnum>()
                .Skip(1);
        }

        /// <summary>
        ///     Fancy printing of messages to a console for notification.
        /// </summary>
        /// <param name="msg">Message to be printed.</param>
        public static void ConsoleNotify(string msg)
        {
            lock (NotifyLock)
            {
                var cc = Console.ForegroundColor; // Save the console's color.
                Console.ForegroundColor = ConsoleColor.DarkRed; // Set the color to Dark Red.
                Console.WriteLine($" !! {msg}");
                Console.ForegroundColor = cc; // Reset the color to the default.
            }
        }

        /// <summary>
        ///     Writes a single string to a log on the Desktop.
        /// </summary>
        /// <param name="logFilename">File to write to or be named.</param>
        /// <param name="logData">Information to place in the log.</param>
        public static void LogWrite(string logFilename, string logData)
        {
            // Prevents log spamming if a repeated error is occuring. 
            if (_badLogAttempts > 5)
                return;

            // If the filename or log information is invalid, do not bother logging.
            if (string.IsNullOrWhiteSpace(logFilename))
            {
                ++_badLogAttempts;
                throw new ArgumentException("File name to save the log is invalid.");
            }

            if (string.IsNullOrWhiteSpace(logData))
            {
                ++_badLogAttempts;
                throw new ArgumentException("Log is invalid or empty.");
            }


            try
            {
                // Creates the 'Desktop' Directory if it does not exist, returns the location.
                var desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop,
                    Environment.SpecialFolderOption.Create);

                // Concatenate "combat.log" to the the Desktop location.
                var fn = Path.Combine(desktopLocation, logFilename);
                using (var sw = File.AppendText(fn))
                {
                    // Appends to the file if it exists, otherwise it will be created and written to.
                    sw.WriteLine($"[{DateTime.Now}]\n{logData}\n"); // Timestamp and log it.
                }

                // For every log attempt that succeeded, we decrease the bad attempt counter.
                if (_badLogAttempts > 0)
                    --_badLogAttempts;
            }
            catch (PlatformNotSupportedException)
            {
                ++_badLogAttempts;
                ConsoleNotify($"Could not log to '{logFilename}' due to the platform not being supported.");
            }
            catch (Exception e)
            {
                ++_badLogAttempts;
                ConsoleNotify($"An unknown error occurred while logging information to '{logFilename}': \n{e}");
            }
        }

        /// <summary>
        ///     Writes a list of strings to a log on the Desktop.
        /// </summary>
        /// <param name="logFilename">File to write to our be named.</param>
        /// <param name="logData">List of data that needs to be placed into the log.</param>
        public static void LogWrite(string logFilename, List<string> logData)
        {
            // Prevents log spamming if a repeated error is occuring. 
            if (_badLogAttempts > 5)
                return;

            // If the filename or log information is invalid, do not bother logging.
            if (string.IsNullOrWhiteSpace(logFilename))
            {
                ++_badLogAttempts;
                throw new ArgumentException("File name to save the log is invalid.");
            }

            if (logData == null || logData.Count == 0)
            {
                ++_badLogAttempts;
                throw new ArgumentException("Log is invalid or empty.");
            }


            try
            {
                // Creates the 'Desktop' Directory if it does not exist, returns the location.
                var desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop,
                    Environment.SpecialFolderOption.Create);

                // Concatenate "combat.log" to the the Desktop location.
                var fn = Path.Combine(desktopLocation, logFilename);
                using (var sw = File.AppendText(fn))
                {
                    // Appends to the file if it exists, otherwise it will be created and written to.
                    sw.WriteLine($"[{DateTime.Now}]"); // Timestamp the log.
                    foreach (var str in logData)
                        sw.WriteLine(str); // Write the server responses to the log.

                    sw.WriteLine(); // Blank line for the next log.
                }

                // For every log attempt that succeeded, we decrease the bad attempt counter.
                if (_badLogAttempts > 0)
                    --_badLogAttempts;
            }
            catch (PlatformNotSupportedException)
            {
                ++_badLogAttempts;
                ConsoleNotify($"Could not log to '{logFilename}' due to the platform not being supported.");
            }

            catch (Exception e)
            {
                ++_badLogAttempts;
                ConsoleNotify($"An unknown error occurred while logging information to '{logFilename}': \n{e}");
            }
        }

        #region RNG

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

            return min + RandomImpl.NextDouble() * (max - min);
        }

        public static int RandomMinMax(int min, int max)
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

            return min + RandomImpl.Next(max - min + 1);
        }

        public static int Random(int from, int count)
        {
            if (count == 0)
                return from;

            if (count > 0)
                return from + RandomImpl.Next(count);

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

        public static TEnum RandomEnum<TEnum>()
        {
            var val = Enum.GetValues(typeof(TEnum));
            return (TEnum) val.GetValue(Random(val.Length));
        }

        #endregion
    }

    /// <summary>
    ///     Prepares and deciphers data transferred over the network.
    /// </summary>
    public static class Network
    {
        private const int HeaderSize = sizeof(long); // A constant that stores length that prefixes the byte array.

        /// <summary>
        ///     Takes an object and converts it to a byte array prefixing its size.
        /// </summary>
        /// <param name="obj">Object to be converted.</param>
        /// <returns>Byte array containing the object.</returns>
        public static byte[] Serialize(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, obj);

                var message = new byte[HeaderSize + memoryStream.Length];
                var header = BitConverter.GetBytes(memoryStream.Length);
                var body = memoryStream.ToArray();
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
        private const int HeaderSize = sizeof(long);
        public const int BufferSize = 1024;

        // Receive buffer.  
        public readonly byte[] Buffer = new byte[BufferSize];

        // Check if we have got the read size.
        private bool _haveSize;

        // Stores the extracted object size.
        public long ObjectSize = -1;

        // Client socket.  
        public Socket Socket;

        // Received data total.
        public byte[] Value;

        // Extracts the size of the object.
        public bool ExtractSize(byte[] array, int size)
        {
            // Return early if it's already been performed. This should be called before attempting this.
            if (_haveSize)
                return true;

            if (ObjectSize >= 0)
                return true;

            if (size < HeaderSize)
                return true;

            // Attempt to get first sizeof(long) bytes from buffer.
            ObjectSize = BitConverter.ToInt64(array, 0);

            // Reassign new buffer with trimmed header.
            var tBuffer = new byte[size - HeaderSize];
            Array.Copy(Buffer, HeaderSize, tBuffer, 0, size - HeaderSize);

            // Add to our current value.
            Add(tBuffer, tBuffer.Length);

            _haveSize = true;
            return false;
        }

        // Add to the current Value.
        public void Add(byte[] array, int size)
        {
            var iLength = 0;
            if (Value != null && Value.Length > 0)
                iLength = Value.Length;

            var newValue = new byte[size + iLength];

            var offset = 0;
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
        public enum Types
        {
            Server = 1,
            Client = 2
        }

        private readonly bool _debug;
        private readonly ManualResetEvent _readDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);
        private readonly Socket _socket;
        private readonly Types _type;
        private IPacket _response;

        /// <summary>
        ///     Creates an instance of a SocketHandler with the information provided.
        /// </summary>
        public SocketHandler(Socket socket, Types type, bool debug = false)
        {
            _type = type;
            _socket = socket;
            _debug = debug;
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            var state = (StateObject) ar.AsyncState;
            var handler = state.Socket;

            // Read data from the client socket.
            int bytesRead;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException)
            {
                Kill(true);
                return;
            }

            if (bytesRead <= 0)
                return;

            // Extract our ObjectSize if we haven't already.
            if (state.ExtractSize(state.Buffer, bytesRead))
                state.Add(state.Buffer, bytesRead);

            if (state.Value.Length == state.ObjectSize)
            {
                if (_debug)
                    Console.WriteLine(
                        $" => {state.Value.Length + sizeof(long)} bytes read from {Enum.GetName(typeof(Types), _type)}.");

                _response = Network.Deserialize(state.Value) as IPacket;
                _readDone.Set();
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = handler.EndSend(ar);

                if (_debug)
                    Console.WriteLine($" <= {bytesSent} bytes sent to {Enum.GetName(typeof(Types), _type)}.");

                _sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Read information from the socket.
        /// </summary>
        /// <returns>Packet received from the remote socket.</returns>
        public IPacket Receive()
        {
            var state = new StateObject
            {
                Socket = _socket
            };
            _socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
            //this.Receive();
            _readDone.WaitOne();

            var obj = _response;
            _response = null;

            _readDone.Reset();

            return obj;
        }

        /// <summary>
        ///     Send information over the socket to the destination.
        /// </summary>
        /// <param name="packet">Information to be setn.</param>
        public void Send(IPacket packet)
        {
            var data = packet.ToByte();

            try
            {
                // Begin sending the data to the remote device.  
                _socket.BeginSend(data, 0, data.Length, 0, SendCallback, _socket);
                _sendDone.WaitOne();

                _sendDone.Reset();
            }
            catch (SocketException se)
            {
                var n = Assembly.GetExecutingAssembly().GetName().Name;
                var t = $"Error occurred in '{n}' while sending data.\n{se.Message}";
                Utility.LogWrite("err.txt", t);
            }
        }

        /// <summary>
        ///     Shuts the socket down. Can send a kill packet to the remote connection.
        /// </summary>
        /// <param name="sendKill">Inform the remote end if it should terminate.</param>
        public void Kill(bool sendKill = false)
        {
            if (sendKill)
                Send(new SocketKillPacket());

            _socket.Close();
        }
    }

    internal sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            var currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            var typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");

            return typeToDeserialize;
        }
    }
}
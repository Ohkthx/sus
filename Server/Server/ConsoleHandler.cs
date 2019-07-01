using System.Net.Sockets;
using SUS.Shared;

namespace SUS.Server.Server
{
    public class ConsoleHandler : IHandler
    {
        #region Constructors

        public ConsoleHandler(Socket socket)
        {
            Handler = new SocketHandler(socket, SocketHandler.Types.Client, true);
        }

        #endregion

        #region Getters / Setters

        private SocketHandler Handler { get; }

        #endregion

        public void Close()
        {
        }

        #region Processor

        public void Core()
        {
        }

        #endregion
    }
}
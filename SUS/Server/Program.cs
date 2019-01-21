namespace SUS.Server
{
    internal static class Program
    {
        private static void Main()
        {
            StartServer();
        }

        // Initiates the server and its networking.
        private static void StartServer()
        {
            ServerInstance.StartListening();
        }
    }
}
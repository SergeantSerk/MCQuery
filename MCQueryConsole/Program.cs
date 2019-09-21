using MCQuery;
using System;

namespace MCQueryConsole
{
    public class Program
    {
        private static string address = "mc.hypixel.net";
        private static ushort port = 25565;

        public static void Main(string[] args)
        {
            MCServer server = new MCServer(address, port);
            string json = server.Status();
            double ping = server.Ping();
            Console.WriteLine($"Server: {server.Address}:{server.Port}");
            Console.WriteLine($"Ping:   {ping}ms");
            Console.WriteLine($"Status: {json}");
        }
    }
}

using MCQuery;
using System;

namespace MCQueryConsole
{
    public class Program
    {
        private static string server = "mc.hypixel.net";
        private static ushort port = 25565;

        public static void Main(string[] args)
        {
            string json = Minecraft.Status(server, port);
            double ping = Minecraft.Ping(server, port);
            Console.WriteLine($"Server: {server}:{port}");
            Console.WriteLine($"Ping:   {ping}ms");
            Console.WriteLine($"Status: {json}");
        }
    }
}

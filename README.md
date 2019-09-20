# MCQuery
A C# library for querying/pinging Minecraft servers.

# Sample Usage
See MCQueryConsole for sample console or below.
```
using MCQuery;
using System;

namespace Sample
{
    public class Program
    {
        // Enter the server address
        private static string server = "mc.hypixel.net";
        // Enter the server port
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
```

# Installation
For the latest features (bleeding-edge), clone this repository and open the **`MCQuery/MCQuery.sln`** solution in Visual Studio. Build using the **Release** configuration and use the **`MCQuery/bin/Release/netcoreapp(x)/MCQuery.dll`** file in your project.

Alternatively, you can download the DLL (not always up-to-date) from the [Release](https://github.com/SergeantSerk/MCQuery/releases) page, or download the MCQuery NuGet package from nuget.org using Visual Studio's NuGet Package Manager.

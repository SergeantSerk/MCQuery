# MCQuery [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery?ref=badge_shield) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/962a271ffb45453f9be089d88021aabc)](https://www.codacy.com/manual/SergeantSerk/MCQuery?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=SergeantSerk/MCQuery&amp;utm_campaign=Badge_Grade)

## Description
A C# library for querying/pinging Minecraft servers.

## Sample Usage
See MCQueryConsole for sample console or below.
```C#
using MCQuery;
using System;

namespace Sample
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
            Console.WriteLine($"Server: {server}:{port}");
            Console.WriteLine($"Ping:   {ping}ms");
            Console.WriteLine($"Status: {json}");
        }
    }
}

```

## Installation
For the latest features (bleeding-edge), clone this repository and open the **`MCQuery/MCQuery.sln`** solution in Visual Studio. Build using the **Release** configuration and use the **`MCQuery/bin/Release/netcoreapp(x)/MCQuery.dll`** file in your project.

Alternatively, you can download the DLL (not always up-to-date) from the [Release](https://github.com/SergeantSerk/MCQuery/releases) page, or download the MCQuery NuGet package from nuget.org using Visual Studio's NuGet Package Manager.

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery?ref=badge_large)

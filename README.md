# MCQuery [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery?ref=badge_shield) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/962a271ffb45453f9be089d88021aabc)](https://www.codacy.com/gh/SergeantSerk/MCQuery/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=SergeantSerk/MCQuery&amp;utm_campaign=Badge_Grade) [![Build status](https://ci.appveyor.com/api/projects/status/q2imiippvb2yotxn?svg=true)](https://ci.appveyor.com/project/SergeantSerk/mcquery)

## Description
A C# library for querying/pinging Minecraft servers.

## Sample Usage
See sample code below for basic usage.
```C#
using MCQuery;
using System;

namespace Sample
{
    public class Program
    {
        private static string address = "mc.hypixel.net";
        private static int port = 25565;

        public static void Main(string[] args)
        {
            MCServer server = new MCServer(address, port);
            ServerStatus status = server.Status();
            double ping = server.Ping();
            Console.WriteLine($"Server: {server.Address}:{server.Port}");
            Console.WriteLine($"Ping:   {ping}ms");
            Console.WriteLine($"Status: {status.Players.Online}");
        }
    }
}
```

## Installation
Recommended way of installing is through [NuGet](https://www.nuget.org/packages/MCQuery/) where packages are updated alongside master branch.

For the latest features (bleeding-edge), clone this repository and open the **`MCQuery/MCQuery.sln`** solution in Visual Studio. Build using the **Release** configuration and use the **`MCQuery/bin/Release/netstandard2.0/MCQuery.dll`** file in your project.

Alternatively, you can download the DLL (not always up-to-date) from the [Release](https://github.com/SergeantSerk/MCQuery/releases) page.

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FSergeantSerk%2FMCQuery?ref=badge_large)

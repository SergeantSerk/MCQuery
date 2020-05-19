using Newtonsoft.Json;

namespace MCQueryTests
{
    internal class ServerStatus
    {
        // Don't care about description, this changes over different server

        [JsonProperty("players")]
        public Players Players { get; set; }

        [JsonProperty("version")]
        public Version Version { get; set; }
    }

    internal class Players
    {
        [JsonProperty("max")]
        public long Max { get; set; }

        [JsonProperty("online")]
        public long Online { get; set; }
    }

    internal class Version
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("protocol")]
        public long Protocol { get; set; }
    }
}
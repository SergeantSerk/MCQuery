using System.Text.Json.Serialization;

namespace MCQueryTests
{
    internal class ServerStatus
    {
        // Don't care about description, this changes over different server

        [JsonPropertyName("players")]
        public Players Players { get; set; }

        [JsonPropertyName("version")]
        public Version Version { get; set; }
    }

    internal class Players
    {
        [JsonPropertyName("max")]
        public long Max { get; set; }

        [JsonPropertyName("online")]
        public long Online { get; set; }
    }

    internal class Version
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("protocol")]
        public long Protocol { get; set; }
    }
}
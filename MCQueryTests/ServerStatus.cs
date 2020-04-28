using System.Text.Json.Serialization;

namespace MCQueryTests
{
    public partial class ServerStatus
    {
        // Don't care about description, this changes over different server

        [JsonPropertyName("players")]
        public Players Players { get; set; }

        [JsonPropertyName("version")]
        public Version Version { get; set; }
    }

    public partial class Players
    {
        [JsonPropertyName("max")]
        public long Max { get; set; }

        [JsonPropertyName("online")]
        public long Online { get; set; }
    }

    public partial class Version
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("protocol")]
        public long Protocol { get; set; }
    }
}
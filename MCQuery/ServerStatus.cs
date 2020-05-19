using Newtonsoft.Json;

namespace MCQuery
{
    public class ServerStatus
    {
        // Don't care about description, this changes over different server

        [JsonProperty("players")]
        public Players Players { get; set; }

        [JsonProperty("version")]
        public Version Version { get; set; }
    }

    public class Players
    {
        [JsonProperty("max")]
        public long Max { get; set; }

        [JsonProperty("online")]
        public long Online { get; set; }
    }

    public class Version
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("protocol")]
        public long Protocol { get; set; }
    }
}
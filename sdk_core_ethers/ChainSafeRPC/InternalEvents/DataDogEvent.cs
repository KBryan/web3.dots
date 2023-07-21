using Newtonsoft.Json;

namespace Web3Unity.Scripts.Library.Ethers.InternalEvents
{
    public class DataDogEvent
    {
        [JsonProperty(PropertyName = "ddsource")]
        public string DD_SOURCE { get; set; }

        [JsonProperty(PropertyName = "ddtags")]
        public object DDTAGS { get; set; }

        [JsonProperty(PropertyName = "hostname")]
        public string HOST_NAME { get; set; }

        [JsonProperty(PropertyName = "message")]
        public object MESSAGE { get; set; }

        [JsonProperty(PropertyName = "service")]
        public string SERVICE { get; set; }

        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "event")] public string Event { get; set; }

        [JsonProperty(PropertyName = "projectID")]
        public string PROJECT_ID { get; set; }
    }
}
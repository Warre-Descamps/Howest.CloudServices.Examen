using Newtonsoft.Json;

namespace Howest.CloudServices.Examen.Models;

public class DroneResult
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("deviceId")] public string DeviceId { get; set; }
    [JsonProperty("target")] public string Target { get; set; }
    [JsonProperty("accuracy")] public float Accuracy { get; set; }
    [JsonProperty("latitude")] public float Latitude { get; set; }
    [JsonProperty("longitude")] public float Longitude { get; set; }
    [JsonProperty("object")] public string Object { get; set; }
    [JsonProperty("processed")] public bool Processed { get; set; }
    [JsonProperty("timestamp")] public DateTime TimeStamp { get; set; }
}
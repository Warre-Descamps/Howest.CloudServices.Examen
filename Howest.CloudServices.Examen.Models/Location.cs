using Newtonsoft.Json;

namespace Howest.CloudServices.Examen.Models;

public class Location
{
    [JsonIgnore] public Guid Id { get; set; }
    [JsonProperty("countryCode")] public string CountryCode { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("latitude")] public float Latitude { get; set; }
    [JsonProperty("longitude")] public float Longitude { get; set; }
    [JsonProperty("person")] public string Person { get; set; }
    [JsonProperty("object")] public string Object { get; set; }
}
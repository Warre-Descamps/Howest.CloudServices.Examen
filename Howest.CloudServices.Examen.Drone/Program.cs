using System.Text;
using Howest.CloudServices.Examen.Drone.Security;
using Howest.CloudServices.Examen.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Howest.CloudServices.Examen.Drone;

internal static class Program
{
    private static float _batteryPercentage = 100;
    private static Location? _target;
    
    public static async Task Main(string[] _)
    {
        var (appSettings, random) = await InitAsync();
        
        await using var deviceClient = DeviceClient.CreateFromConnectionString(appSettings.ConnectionString);
        await deviceClient.OpenAsync();
        
        var reportedProperties = new TwinCollection
        {
            ["bootTime"] = DateTime.Now,
            ["battery"] = _batteryPercentage
        };
        await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        await ForceDeviceTwinRetrievalAsync(deviceClient);
        
        await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null);
        
        while (true)
        {
            reportedProperties = new TwinCollection
            {
                ["battery"] = _batteryPercentage -= (float)(random.NextDouble() * .25)
            };
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

            if (_batteryPercentage < 1)
                break;

            await SendMessageAsync(deviceClient, random, appSettings.DeviceId);
            await Task.Delay(1000);
        }
    }

    private static async Task ForceDeviceTwinRetrievalAsync(DeviceClient deviceClient)
    {
        var twin = await deviceClient.GetTwinAsync();

        await OnDesiredPropertyChanged(twin.Properties.Desired, deviceClient);
    }

    private static Task OnDesiredPropertyChanged(TwinCollection desiredproperties, object usercontext)
    {
        if (!desiredproperties.Contains("target"))
        {
            throw new Exception("Drone cannot be started. No target set.");
        }
            
        var data = desiredproperties["target"];
        _target = JsonConvert.DeserializeObject<Location>(data.ToString());
        if (_target is null)
        {
            throw new Exception("Drone cannot be started. No target set.");
        }
        Console.WriteLine($"One or more device twin properties changed: {JsonConvert.SerializeObject(desiredproperties)}");
        return Task.CompletedTask;
    }

    private static async Task<(AppSettings, Random)> InitAsync()
    {
        var file = JsonConvert.DeserializeObject<JToken>(await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) ?? throw new Exception();
        var appsettings = JsonConvert.DeserializeObject<AppSettings>(file[nameof(AppSettings)]?.ToString() ?? throw new Exception()) ?? throw new Exception();
        var random = new Random();

        return (appsettings, random);
    }

    private static Task SendMessageAsync(DeviceClient deviceClient, Random random, string deviceId)
    {
        var data = new DroneResult
        {
            DeviceId = deviceId,
            Target = _target!.Id.ToString(),
            Accuracy = random.NextSingle() * 100,
            Latitude = _target.Latitude,
            Longitude = _target.Longitude,
            Object = _target.Object,
            Processed = false,
            TimeStamp = DateTime.UtcNow
        };
        
        var jsonData = JsonConvert.SerializeObject(data);
        var message = new Message(Encoding.UTF8.GetBytes(jsonData));

        return deviceClient.SendEventAsync(message);
    }
}
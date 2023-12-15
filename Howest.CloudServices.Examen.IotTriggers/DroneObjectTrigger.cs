using System;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using Howest.CloudServices.Examen.Models;
using Howest.Mct.Functions.CosmosDb.Helper;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Howest.CloudServices.Examen.IotTriggers;

public static class DroneObjectTrigger
{
    [FunctionName("DroneObjectDetected")]
    public static async Task RunAsync([IoTHubTrigger("messages/events", Connection = "EventHubEndPoint")] EventData message,
        ILogger log)
    {
        var dataString = Encoding.UTF8.GetString(message.Body.ToArray());
        var data = JsonConvert.DeserializeObject<DroneResult>(dataString);

        if (data is null)
            return;

        log.LogInformation($"{data.DeviceId} detected a {data.Object} with accuracy {data.Accuracy}");
        if (data.Accuracy < 98)
        {
            return;
        }
        
        var container = CosmosHelper.GetContainer();
        
        data.Id = Guid.NewGuid();
        await container.CreateItemAsync(data, new PartitionKey(data.DeviceId));
    }
}
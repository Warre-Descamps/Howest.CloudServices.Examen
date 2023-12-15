using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Howest.CloudServices.Examen.Models;
using Howest.Mct.Functions.CosmosDb.Helper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Howest.CloudServices.Examen.TimerTriggers;

public static class ProcessDetectionsTrigger
{
    private const string MessageBody = @"{{ ""to"": ""{0}"", ""target "": ""{1}"", ""objectDetected"": ""{2}"" }}";
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("https://prod-170.westeurope.logic.azure.com/workflows/b32ae7cc7dfe48b1ba30d38a5e5129e9/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=msv5rU-PaX16C6zVKkiW5wHwJ51Lu4hFmMbmhl-D6Y0")
    };
    
    [FunctionName("ProcessDetectionsTrigger")]
    public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
    {
        var container = CosmosHelper.GetContainer();

        var items = await container.GetItems<DroneResult>()
            .Where(r => !r.Processed)
            .ToListAsync();

        foreach (var item in items)
        {
            var content = new StringContent(string.Format(MessageBody, "user@example.com", item.Target, item.Object), Encoding.UTF8, "application/json");
            await HttpClient.PostAsync("", content);
            
            item.Processed = true;
            await container.UpsertItemAsync(item, new PartitionKey(item.DeviceId));
        }
    }
}
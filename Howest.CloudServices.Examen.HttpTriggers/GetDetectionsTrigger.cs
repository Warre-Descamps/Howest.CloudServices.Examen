using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Howest.CloudServices.Examen.Models;
using Howest.Mct.Functions.CosmosDb.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Howest.CloudServices.Examen.HttpTriggers;

public static class GetDetectionsTrigger
{
    [FunctionName("GetDetectionsTrigger")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "detections")] HttpRequest req, ILogger log)
    {
        var container = CosmosHelper.GetContainer();
        var items = await container.GetItems<DroneResult>().ToListAsync();
        return new OkObjectResult(items);
    }
}
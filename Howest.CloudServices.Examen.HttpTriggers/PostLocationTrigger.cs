using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Howest.CloudServices.Examen.Helpers;
using Howest.CloudServices.Examen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Howest.CloudServices.Examen.HttpTriggers;

public static class PostLocationTrigger
{
    [FunctionName("PostLocationTrigger")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "location")] HttpRequest req, ILogger log)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = JsonConvert.DeserializeObject<Location>(requestBody);
        
        if (request is null)
        {
            return new BadRequestResult();
        }
        
        var tableClient = new TableClient(EnvironmentHelper.ConnectionString, EnvironmentHelper.TableName);
        await tableClient.CreateIfNotExistsAsync();

        request.Id = Guid.NewGuid();
        var entity = new TableEntity(EnvironmentHelper.PartitionKey, Guid.NewGuid().ToString())
        {
            { "id", request.Id },
            { "countryCode", request.CountryCode },
            { "name", request.Name },
            { "latitude", request.Latitude },
            { "longitude", request.Longitude },
            { "person", request.Person },
            { "object", request.Object }
        };

        await tableClient.AddEntityAsync(entity);

        return new OkObjectResult(request);
        
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Data.Tables;
using Howest.CloudServices.Examen.Helpers;
using Howest.CloudServices.Examen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;

namespace Howest.CloudServices.Examen.HttpTriggers;

public static class PutDroneLocationTrigger
{
    [FunctionName("PutDroneLocation")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "{deviceId}/{countryCode}/{targetId:Guid}")] HttpRequest req, ILogger log,
        string deviceId, string countryCode, Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(countryCode))
        {
            return new BadRequestResult();
        }

        var tableClient = new TableClient(EnvironmentHelper.ConnectionString, EnvironmentHelper.TableName);

        try
        {
            var location = await tableClient
                .QueryAsync<TableEntity>(
                    filter:
                    $"PartitionKey eq '{EnvironmentHelper.PartitionKey}' and countryCode eq '{countryCode}' and id eq guid'{targetId}'")
                .Select(e =>
                {
                    var id = e["id"].ToString();
                    var cc = e["countryCode"].ToString();
                    var name = e["name"].ToString();
                    var latitude = Convert.ToSingle(e["latitude"]);
                    var longitude = Convert.ToSingle(e["longitude"]);
                    var person = e["person"].ToString();
                    var obj = e["object"].ToString();

                    if (new[] { id, cc, name, person, obj }.Any(string.IsNullOrWhiteSpace))
                        return null;

                    return new Location
                    {
                        Id = Guid.Parse(id!),
                        CountryCode = cc!,
                        Name = name!,
                        Latitude = latitude,
                        Longitude = longitude,
                        Person = person!,
                        Object = obj!
                    };
                })
                .Cast<Location>()
                .SingleOrDefaultAsync();

            if (location is null)
            {
                return new NotFoundObjectResult("Target");
            }

            var manager = RegistryManager.CreateFromConnectionString(EnvironmentHelper.IotHubAdmin);

            var twin = await manager.GetTwinAsync(deviceId);
            if (twin is null)
            {
                return new NotFoundObjectResult("Device");
            }

            twin.Properties.Desired["target"] = location;
            await manager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
            return new OkResult();
        }
        catch (InvalidOperationException)
        {
            // In this case you should definitely send an email to the system manager. But I aint no time for that atm.
            return new InternalServerErrorResult();
        }
        catch (Exception e)
        {
            log.LogCritical(e.Message);
            return new InternalServerErrorResult();
        }
    }
}
namespace Howest.CloudServices.Examen.Helpers;

public static class EnvironmentHelper
{
    private const string Error = "{0} is a required variable in local.settings.json!";
    public static string TableName => Environment.GetEnvironmentVariable(nameof(TableName)) ?? 
                                      throw new ArgumentNullException(string.Format(Error, nameof(TableName)));

    public static string PartitionKey => Environment.GetEnvironmentVariable(nameof(PartitionKey)) ??
                                         throw new ArgumentNullException(string.Format(Error, nameof(PartitionKey)));
    
    public static string ConnectionString => Environment.GetEnvironmentVariable(nameof(ConnectionString)) ??
                                             throw new ArgumentNullException(string.Format(Error, nameof(ConnectionString)));

    public static string IotHubAdmin => Environment.GetEnvironmentVariable(nameof(IotHubAdmin)) ??
                                        throw new ArgumentNullException(string.Format(Error, nameof(IotHubAdmin)));
}
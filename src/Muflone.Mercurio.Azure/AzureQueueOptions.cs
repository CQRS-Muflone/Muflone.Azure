namespace Muflone.Mercurio.Azure;

public class AzureQueueOptions
{
    public string PrimaryConnectionString { get; set; }
    public string ServiceBusNameSpace { get; set; }
    public string RegionName { get; set; }
    public string ResourceGroup { get; set; }
    public string QueueName { get; set; }
}
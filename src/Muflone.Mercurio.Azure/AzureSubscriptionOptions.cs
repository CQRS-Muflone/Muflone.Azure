
namespace Muflone.Mercurio.Azure;

public class AzureSubscriptionOptions
{
    public string PrimaryConnectionString { get; set; }
    public string ServiceBusName { get; set; }
    public string Region { get; set; }
    public string ResourceGroup { get; set; }
    public string Scale { get; set; }
    public string TopicName { get; set; }
    public string QueueCommandName { get; set; }
    public string SubscriptionName { get; set; }
}
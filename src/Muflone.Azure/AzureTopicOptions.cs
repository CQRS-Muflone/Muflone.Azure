namespace Muflone.Azure;

public class AzureTopicOptions
{
    public string PrimaryConnectionString { get; set; }
    public string ServiceBusNameSpace { get; set; }
    public string RegionName { get; set; }
    public string ResourceGroup { get; set; }
    public string SubscriptionName { get; set; }
    public string TopicName { get; set; }
}
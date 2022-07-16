namespace Muflone.Azure.Factories;

public class BrokerOptions
{
	public string ConnectionString { get; set; }
	public string QueueName { get; set; }
	public string TopicName { get; set; }
	public string SubscriptionName { get; set; }
}
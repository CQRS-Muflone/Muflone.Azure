using Azure.Messaging.EventHubs;
using Muflone.Messages;

namespace Muflone.Azure.EventHub;

public interface IIoTMapper
{
	Message MapToAthena(EventData azureMessage);
	Microsoft.Azure.Devices.Client.Message MapToAzure(Message athenaMessage);
}
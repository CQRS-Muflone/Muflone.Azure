using Azure.Messaging.EventGrid;
using Muflone.Messages;

namespace Muflone.Azure.EventGrid;

public interface IIoTMapper
{
	Message MapToAthena(EventGridEvent azureMessage);
	EventGridEvent MapToAzure(Message athenaMessage);
}
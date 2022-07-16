using System;
using System.Text;
using Azure.Messaging.EventHubs;
using Muflone.Messages;
using Muflone.Messages.Enums;

namespace Muflone.Azure.EventHub;

public class IoTMapper : IIoTMapper
{
	public Message MapToAthena(EventData azureMessage)
	{
		var eventArray = azureMessage.Body.ToArray();
		if (eventArray == null)
			throw new Exception("IoT EventData Invalid!!!");

		var eventString = Encoding.UTF8.GetString(eventArray);

		return new Message(new MessageHeader(Guid.NewGuid(), string.Empty, MessageType.MtNone, DateTime.UtcNow),
			new MessageBody(eventString));
	}

	public Microsoft.Azure.Devices.Client.Message MapToAzure(Message athenaMessage)
	{
		return new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(athenaMessage.Body.Value));
	}
}
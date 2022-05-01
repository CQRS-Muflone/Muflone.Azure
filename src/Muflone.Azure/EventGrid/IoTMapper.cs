using System;
using Azure.Messaging.EventGrid;
using Muflone.Messages;
using Muflone.Messages.Enums;

namespace Muflone.Azure.EventGrid;

public class IoTMapper : IIoTMapper
{
    public Message MapToAthena(EventGridEvent azureMessage)
    {
        var eventArray = @azureMessage.Data;
        if (eventArray == null)
            throw new Exception("IoT EventData Invalid!!!");

        var eventString = eventArray.ToString();

        return new Message(new MessageHeader(Guid.NewGuid(), string.Empty, MessageType.MtNone, DateTime.UtcNow), new MessageBody(eventString));
    }

    public EventGridEvent MapToAzure(Message athenaMessage)
    {
        return new (athenaMessage.Header.Topic, athenaMessage.Header.ContentType, "1.0", null);
    }
}
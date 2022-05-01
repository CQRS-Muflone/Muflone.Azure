using System;

namespace Muflone.Azure.Abstracts;

public class EventExceptionQueueArgs : EventArgs
{
    public readonly string CommandName;
    public readonly string ErrorMessage;
    public readonly string MessageId;

    public EventExceptionQueueArgs(string commandName, string errorMessage, string messageId)
    {
        CommandName = commandName;
        ErrorMessage = errorMessage;
        MessageId = messageId;
    }
}
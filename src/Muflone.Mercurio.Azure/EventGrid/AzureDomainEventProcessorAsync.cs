using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Muflone.Mercurio.Azure.Exceptions;
using Muflone.Mercurio.Azure.Factories;
using Muflone.Messages;
using Muflone.Messages.Enums;
using Muflone.Messages.Events;
using Newtonsoft.Json;

namespace Muflone.Mercurio.Azure.EventGrid;

public class AzureDomainEventProcessorAsync<T> : IDomainEventProcessorAsync<T> where T : class, IDomainEvent
{
    public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

    private readonly EventGridPublisherClient _eventGridPublisherClient;
    private readonly IMessageMapper<T> _messageMapper;
    private readonly IDomainEventHandlerAsync<T> _eventHandlerAsync;

    internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
        IMessageMapper<T> messageMapper)
    {
        _messageMapper = messageMapper;

        if (!string.IsNullOrEmpty(brokerOptions.TopicName))
        {
            var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName ?? string.Empty);
            _eventGridPublisherClient =
                new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
        }
    }

    internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions)
    {
        if (!string.IsNullOrEmpty(brokerOptions.TopicName))
        {
            var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName ?? string.Empty);
            _eventGridPublisherClient =
                new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
        }
    }

    public void RegisterBroker()
    {
        if (_eventHandlerAsync == null)
            throw new Exception($"No DomainEventHandler has found for {typeof(T)}. At least one DomainEventHandler must be specified for every DomainEvent");
    }

    public Task HandleAsync(Message message, CancellationToken token = new ())
    {
        throw new NotImplementedException();
    }

    public async Task PublishAsync(T domainEvent, CancellationToken token = new ())
    {
        try
        {
            var athenaMessage = _messageMapper != null
                ? _messageMapper.MapToMessage(domainEvent)
                : new Message(
                    new MessageHeader(domainEvent.MessageId, string.Empty, MessageType.MtNone, new Guid?(),
                        (string) null,
                        "text/plain"), new MessageBody(JsonConvert.SerializeObject((object) domainEvent), "JSON"));

            await _eventGridPublisherClient.SendEventsAsync(new List<EventGridEvent>
            {
                MapAthenaMessageToAzure(athenaMessage)
            }, token);

        }
        catch (Exception ex)
        {
            OnException(new MufloneExceptionArgs(ex));
        }
    }

    #region Helpers
    protected virtual void OnException(MufloneExceptionArgs e)
    {
        var handler = MufloneExceptionHandler;
        handler?.Invoke(this, e);
    }

    public EventGridEvent MapAthenaMessageToAzure(Message athenaMessage)
    {
        return new (athenaMessage.Header.Topic, athenaMessage.Header.ContentType, "1.0",
            athenaMessage.Body);
    }
    #endregion
}
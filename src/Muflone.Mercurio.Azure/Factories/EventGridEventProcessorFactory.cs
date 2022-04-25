using Muflone.Factories;
using Muflone.Mercurio.Azure.EventGrid;
using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Factories;

public class EventGridEventProcessorFactory<T> where T : class, IDomainEvent
{
    public readonly IDomainEventProcessorAsync<T> DomainEventProcessorAsync;

    public EventGridEventProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

        DomainEventProcessorAsync = messageMapper != null
            ? new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper)
            : new AzureDomainEventProcessorAsync<T>(brokerOptions);
    }
}
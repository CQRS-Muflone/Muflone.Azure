using Muflone.Azure.EventGrid;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

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
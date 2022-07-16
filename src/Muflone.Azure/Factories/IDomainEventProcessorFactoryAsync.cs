using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IDomainEventProcessorFactoryAsync
{
	IDomainEventProcessorAsync<T> CreateDomainEventEventProcessorAsync<T>() where T : class, IDomainEvent;
}
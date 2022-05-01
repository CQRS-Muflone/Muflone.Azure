using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IDomainEventProcessorFactoryAsync
{
    IDomainEventProcessorAsync<T> CreatedomainEventEventProcessorAsync<T>() where T : class, IDomainEvent;
}
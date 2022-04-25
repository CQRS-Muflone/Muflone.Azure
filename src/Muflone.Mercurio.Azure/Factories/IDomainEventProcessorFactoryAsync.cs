using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Factories;

public interface IDomainEventProcessorFactoryAsync
{
    IDomainEventProcessorAsync<T> CreatedomainEventEventProcessorAsync<T>() where T : class, IDomainEvent;
}
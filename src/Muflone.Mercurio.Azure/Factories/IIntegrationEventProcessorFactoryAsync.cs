using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Factories;

public interface IIntegrationEventProcessorFactoryAsync
{
    IIntegrationEventProcessorAsync<T> CreateIntegrationEventProcessorAsync<T>() where T : class, IIntegrationEvent;
}
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IIntegrationEventProcessorFactoryAsync
{
	IIntegrationEventProcessorAsync<T> CreateIntegrationEventProcessorAsync<T>() where T : class, IIntegrationEvent;
}
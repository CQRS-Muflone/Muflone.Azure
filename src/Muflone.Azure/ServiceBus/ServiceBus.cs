using System;
using System.Threading.Tasks;
using Muflone.Azure.Factories;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Azure.Abstracts;
using Muflone.Messages.Events;

namespace Muflone.Azure.ServiceBus;

public class ServiceBus : IServiceBus, IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task SendAsync<T>(T command) where T : class, ICommand
    {
        var commandConsumer = this._serviceProvider.GetService<ICommandProcessorAsync<T>>();
        if (commandConsumer == null)
            throw new Exception($"[ServiceBus.SendAsync] - No Command consumer for {command}");

        await commandConsumer.SendAsync(command);
    }

    public Task RegisterHandlerAsync<T>(Action<T> handler) where T : IMessage
    {
        return Task.CompletedTask;
    }

    public async Task PublishAsync(IMessage @event)
    {
        if (@event.GetType() == typeof(DomainEvent))
            await PublishDomainEventAsync((IDomainEvent)@event);

        if (@event.GetType() == typeof(IntegrationEvent))
            await PublishIntegrationEventAsync((IntegrationEvent)@event);
    }

    private async Task PublishDomainEventAsync<T>(T domainEvent) where T : IDomainEvent
    {
        var eventConsumer = this._serviceProvider.GetService<IDomainEventProcessorAsync<T>>();
        if (eventConsumer == null)
        {
            throw new Exception($"[ServiceBus.PublishDomainEventAsync] - No Event consumer for {domainEvent}");
        }

        await eventConsumer.PublishAsync(domainEvent);
    }

    private async Task PublishIntegrationEventAsync<T>(T integrationEvent) where T : IIntegrationEvent
    {
        var eventConsumer = this._serviceProvider.GetService<IIntegrationEventProcessorAsync<T>>();
        if (eventConsumer == null)
        {
            throw new Exception($"[ServiceBus.PublishIntegrationEventAsync] - No Event consumer for {integrationEvent}");
        }

        await eventConsumer.PublishAsync(integrationEvent);
    }
}
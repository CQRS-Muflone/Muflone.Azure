using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Mercurio.Azure.Abstracts;
using Muflone.Mercurio.Azure.Exceptions;
using Muflone.Mercurio.Azure.Factories;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Subscriptions;

public class RegisterHandlersAsync : IRegisterHandlersAsync
{
    private readonly ICommandProcessorFactoryAsync _commandProcessorFactory;
    private readonly IDomainEventProcessorFactoryAsync _domainEventProcessorFactory;
    private readonly IIntegrationEventProcessorFactoryAsync _integrationEventProcessorFactory;

    public string Message { get; private set; }
    public string StackTrace { get; private set; }
    public string Source { get; private set; }

    public RegisterHandlersAsync(ICommandProcessorFactoryAsync commandProcessorFactory,
        IDomainEventProcessorFactoryAsync domainEventProcessorFactory,
        IIntegrationEventProcessorFactoryAsync integrationEventProcessorFactory)
    {
        _commandProcessorFactory = commandProcessorFactory;
        _domainEventProcessorFactory = domainEventProcessorFactory;
        _integrationEventProcessorFactory = integrationEventProcessorFactory;
    }

    public RegisterHandlersAsync(IServiceProvider serviceProvider)
    {
        _commandProcessorFactory = serviceProvider.GetService<ICommandProcessorFactoryAsync>();
        _domainEventProcessorFactory = serviceProvider.GetService<IDomainEventProcessorFactoryAsync>();
        _integrationEventProcessorFactory =
            serviceProvider.GetService<IIntegrationEventProcessorFactoryAsync>();
    }

    public void RegisterCommandHandler<T>() where T : class, ICommand
    {
        try
        {
            var commandProcessor = _commandProcessorFactory.CreateCommandProcessorAsync<T>();
            commandProcessor.MufloneExceptionHandler += MufloneExceptionHandler;
            commandProcessor.RegisterBroker();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            throw;
        }
    }

    public void RegisterDomainEventHandler<T>() where T : class, IDomainEvent
    {
        try
        {
            var domainEventProcessor = _domainEventProcessorFactory.CreatedomainEventEventProcessorAsync<T>();
            domainEventProcessor.MufloneExceptionHandler += MufloneExceptionHandler;
            domainEventProcessor.RegisterBroker();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            throw;
        }
    }

    public void RegisterIntegrationEventHandler<T>() where T : class, IIntegrationEvent
    {
        try
        {
            var integrationEventprocessor =
                _integrationEventProcessorFactory.CreateIntegrationEventProcessorAsync<T>();
            integrationEventprocessor.MufloneExceptionHandler += MufloneExceptionHandler;
            integrationEventprocessor.RegisterBroker();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            throw;
        }
    }

    public virtual void MufloneExceptionHandler(object sender, MufloneExceptionArgs e)
    {
        Message = e.Message;
        Source = e.Source;
        StackTrace = e.StackTrace;
    }
}
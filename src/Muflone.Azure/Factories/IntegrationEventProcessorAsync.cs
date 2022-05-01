using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class IntegrationEventProcessorAsync<T> : IIntegrationEventProcessorAsync<T> where T : class, IIntegrationEvent
{
    public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

    private readonly BrokerOptions _brokerOptions;
    private readonly IMessageMapper<T> _messageMapper;
    private readonly IIntegrationEventHandlerAsync<T> _integrationEventHandlerAsync;

    internal IntegrationEventProcessorAsync(BrokerOptions brokerOptions,
        IMessageMapper<T> messageMapper,
        IIntegrationEventHandlerAsync<T> integrationEventHandlerAsync)
    {
        _brokerOptions = brokerOptions;

        _messageMapper = messageMapper;
        _integrationEventHandlerAsync = integrationEventHandlerAsync;
    }

    internal IntegrationEventProcessorAsync(BrokerOptions brokerOptions,
        IMessageMapper<T> messageMapper)
    {
        _brokerOptions = brokerOptions;
        _messageMapper = messageMapper;
    }

    public virtual void RegisterBroker()
    {
        if (_integrationEventHandlerAsync == null)
            throw new Exception($"No IntegrationEventHandler has found for {typeof(T)}. At least one IntegrationEventHandler must be specified for every IntegrationEvent");
    }

    public virtual async Task HandleAsync(Message message, CancellationToken token = default)
    {
        try
        {
            // Map the message
            var domainEvent = _messageMapper.MapToRequest(message);
            // Process the domainEvent
            await _integrationEventHandlerAsync.HandleAsync(domainEvent, token);
        }
        catch (Exception ex)
        {
            OnExceptionHandler(new MufloneExceptionArgs(ex));
        }
    }

    public virtual Task PublishAsync(T domainEvent, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnExceptionHandler(MufloneExceptionArgs e)
    {
        var handler = MufloneExceptionHandler;
        handler?.Invoke(this, e);
    }
}
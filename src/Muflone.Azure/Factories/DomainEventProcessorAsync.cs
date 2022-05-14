using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class DomainEventProcessorAsync<T> : IDomainEventProcessorAsync<T> where T : class, IDomainEvent
{
	private readonly BrokerOptions _brokerOptions;
	private readonly IDomainEventHandlerAsync<T> _domainEventHandlerAsync;
	private readonly IMessageMapper<T> _messageMapper;

	internal DomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IDomainEventHandlerAsync<T> eventHandlerAsync)
	{
		_brokerOptions = brokerOptions;

		_messageMapper = messageMapper;
		_domainEventHandlerAsync = eventHandlerAsync;
	}

	internal DomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		_brokerOptions = brokerOptions;
		_messageMapper = messageMapper;
	}

	public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

	public virtual void RegisterBroker()
	{
		if (_domainEventHandlerAsync == null)
			throw new Exception(
				$"No DomainEventHandler has found for {typeof(T)}. At least one DomainEventHandler must be specified for every DomainEvent");
	}

	public virtual async Task HandleAsync(Message message, CancellationToken token = default)
	{
		try
		{
			// Map the message
			var domainEvent = _messageMapper.MapToRequest(message);
			// Process the domainEvent
			await _domainEventHandlerAsync.HandleAsync(domainEvent, token);
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
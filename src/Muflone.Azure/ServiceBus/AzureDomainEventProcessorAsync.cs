using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Muflone.Azure.Exceptions;
using Muflone.Azure.Factories;
using Muflone.Messages;
using Muflone.Messages.Enums;
using Muflone.Messages.Events;
using Newtonsoft.Json;

namespace Muflone.Azure.ServiceBus;

public class AzureDomainEventProcessorAsync<T> : IDomainEventProcessorAsync<T> where T : class, IDomainEvent
{
	private readonly IDomainEventHandlerAsync<T> _eventHandlerAsync;
	private readonly IEnumerable<IDomainEventHandlerAsync<T>> _eventHandlersAsync;

	private readonly IMessageMapper<T> _messageMapper;

	private readonly ServiceBusProcessor _serviceBusProcessor;
	private readonly ServiceBusSender _serviceBusSender;

	public AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IEnumerable<IDomainEventHandlerAsync<T>> eventHandlersAsync)
	{
		var azureTopicOptions = new AzureTopicOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			TopicName = brokerOptions.TopicName,
			SubscriptionName = brokerOptions.SubscriptionName
		};
		CreateTopicIfNotExist(azureTopicOptions);

		_messageMapper = messageMapper;
		var domainEventHandlersArray = eventHandlersAsync as IDomainEventHandlerAsync<T>[] ?? eventHandlersAsync.ToArray();
		_eventHandlerAsync = domainEventHandlersArray.First();
		_eventHandlersAsync = domainEventHandlersArray;

		var serviceBusClient = new ServiceBusClient(azureTopicOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureTopicOptions.TopicName);
	}

	public AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IEnumerable<IDomainEventHandlerAsync<T>> eventHandlersAsync)
	{
		var azureTopicOptions = new AzureTopicOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			TopicName = brokerOptions.TopicName,
			SubscriptionName = brokerOptions.SubscriptionName
		};
		CreateTopicIfNotExist(azureTopicOptions);

		var domainEventHandlersArray = eventHandlersAsync as IDomainEventHandlerAsync<T>[] ?? eventHandlersAsync.ToArray();
		_eventHandlerAsync = domainEventHandlersArray.First();
		_eventHandlersAsync = domainEventHandlersArray;

		var serviceBusClient = new ServiceBusClient(azureTopicOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureTopicOptions.TopicName);
	}

	public AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		var azureTopicOptions = new AzureTopicOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			TopicName = brokerOptions.TopicName,
			SubscriptionName = brokerOptions.SubscriptionName
		};
		CreateTopicIfNotExist(azureTopicOptions);

		_messageMapper = messageMapper;

		var serviceBusClient = new ServiceBusClient(azureTopicOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureTopicOptions.TopicName);
	}

	public AzureDomainEventProcessorAsync(BrokerOptions brokerOptions)
	{
		var azureTopicOptions = new AzureTopicOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			TopicName = brokerOptions.TopicName,
			SubscriptionName = brokerOptions.SubscriptionName
		};
		CreateTopicIfNotExist(azureTopicOptions);

		var serviceBusClient = new ServiceBusClient(azureTopicOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureTopicOptions.TopicName);
	}

	public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

	public void RegisterBroker()
	{
		if (!_eventHandlersAsync.Any())
			throw new Exception(
				$"No DomainEventHandler has found for {typeof(T)}. At least one DomainEventHandler must be specified for every DomainEvent");

		StartProcessingAsync().GetAwaiter().GetResult();
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = new())
	{
		if (cancellationToken.IsCancellationRequested)
			cancellationToken.ThrowIfCancellationRequested();

		try
		{
			// Map the message
			var domainEvent = _messageMapper != null
				? _messageMapper.MapToRequest(message)
				: JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body.Bytes));

			// Process domainEvent
			if (_eventHandlersAsync.Any())
				foreach (var eventHandlerAsync in _eventHandlersAsync)
					await eventHandlerAsync.HandleAsync(domainEvent, cancellationToken);
			else
				await _eventHandlerAsync.HandleAsync(domainEvent, cancellationToken);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
			throw;
		}
	}

	public async Task PublishAsync(T domainEvent, CancellationToken cancellationToken = new())
	{
		if (cancellationToken.IsCancellationRequested)
			cancellationToken.ThrowIfCancellationRequested();

		try
		{
			await _serviceBusSender.SendMessageAsync(MapAthenaMessageToServiceBusMessage(domainEvent),
				cancellationToken);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
		}
	}

	private async Task StartProcessingAsync()
	{
		await _serviceBusProcessor.StartProcessingAsync().ConfigureAwait(false);
	}

	private async Task MessageProcessorHandler(ProcessMessageEventArgs args)
	{
		try
		{
			await ConsumeMessagesAsync(args);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
		}
	}

	private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
	{
		OnException(new MufloneExceptionArgs(arg.Exception));
		return Task.CompletedTask;
	}

	public async Task HandleAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken = new())
	{
		if (cancellationToken.IsCancellationRequested)
			cancellationToken.ThrowIfCancellationRequested();

		try
		{
			var athenaMessage = MapAzureMessageToAthena(args.Message);
			// Map the message
			var domainEvent = _messageMapper != null
				? _messageMapper.MapToRequest(athenaMessage)
				: JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(athenaMessage.Body.Bytes));

			var allHandlersAreCompleted = true;
			// Process domainEvent
			if (_eventHandlersAsync.Any())
				try
				{
					foreach (var eventHandlerAsync in _eventHandlersAsync)
						await eventHandlerAsync.HandleAsync(domainEvent, cancellationToken);
				}
				catch (Exception ex)
				{
					Thread.Sleep(1000);
					if (args.Message.DeliveryCount > 5)
						await args.DeadLetterMessageAsync(args.Message, null, cancellationToken).ConfigureAwait(false);
					else
						await args.AbandonMessageAsync(args.Message, null, cancellationToken).ConfigureAwait(false);

					allHandlersAreCompleted = false;
					OnException(new MufloneExceptionArgs(ex));
				}

			if (allHandlersAreCompleted)
				await args.CompleteMessageAsync(args.Message, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
			throw;
		}
	}

	#region Helpers

	// Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
	// If subscriptionClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
	// to avoid unnecessary exceptions.
	private async Task ConsumeMessagesAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			cancellationToken.ThrowIfCancellationRequested();

		try
		{
			await HandleAsync(args, cancellationToken);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
			throw;
		}
	}

	protected virtual void OnException(MufloneExceptionArgs e)
	{
		var handler = MufloneExceptionHandler;
		handler?.Invoke(this, e);
	}

	private Message MapAzureMessageToAthena(ServiceBusReceivedMessage azureMessage)
	{
		try
		{
			return new Message(
				new MessageHeader(azureMessage.MessageId.ToGuid(), string.Empty, MessageType.MtCommand,
					DateTime.UtcNow), new MessageBody(azureMessage.Body.ToArray(), "JSON"));
		}
		catch (Exception ex)
		{
			var message =
				$"Exception was raised while executing MapAzureMessageToAthena {ex.Message}";

			OnException(new MufloneExceptionArgs(new Exception(message)));
			throw new Exception(message);
		}
	}

	private static ServiceBusMessage MapAthenaMessageToServiceBusMessage(IMessage athenaMessage)
	{
		var message = new ServiceBusMessage(JsonConvert.SerializeObject(athenaMessage))
		{
			MessageId = athenaMessage.MessageId.ToString(),
			SessionId = Guid.NewGuid().ToString()
		};

		if (athenaMessage.UserProperties is null)
			return message;

		foreach (var messageUserProperty in athenaMessage.UserProperties)
			message.ApplicationProperties.Add(messageUserProperty.Key, messageUserProperty.Value);

		return message;
	}

	private static void CreateTopicIfNotExist(AzureTopicOptions azureTopicOptions)
	{
		using var serviceBusAdministraot = new ServiceBusAdmnistrator();
		serviceBusAdministraot.CreateTopicIfNotExistAsync(azureTopicOptions).GetAwaiter().GetResult();
	}

	#endregion
}
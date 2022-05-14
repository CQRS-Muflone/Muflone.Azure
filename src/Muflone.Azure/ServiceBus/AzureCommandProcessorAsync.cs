using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Muflone.Azure.Exceptions;
using Muflone.Azure.Factories;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Enums;
using Newtonsoft.Json;

namespace Muflone.Azure.ServiceBus;

public class AzureCommandProcessorAsync<T> : ICommandProcessorAsync<T> where T : class, ICommand
{
	private readonly ICommandHandlerAsync<T> _commandHandlerAsync;

	private readonly IMessageMapper<T> _messageMapper;

	private readonly ServiceBusProcessor _serviceBusProcessor;
	private readonly ServiceBusSender _serviceBusSender;

	internal AzureCommandProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		ICommandHandlerAsync<T> commandHandlerAsync)
	{
		var azureQueueOptions = new AzureQueueOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			QueueName = brokerOptions.QueueName
		};
		CreateQueueIfNotExist(azureQueueOptions);

		_messageMapper = messageMapper;
		_commandHandlerAsync = commandHandlerAsync;

		var serviceBusClient = new ServiceBusClient(azureQueueOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureQueueOptions.QueueName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureQueueOptions.QueueName);
	}

	internal AzureCommandProcessorAsync(BrokerOptions brokerOptions,
		ICommandHandlerAsync<T> commandHandlerAsync)
	{
		var azureQueueOptions = new AzureQueueOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			QueueName = brokerOptions.QueueName
		};
		CreateQueueIfNotExist(azureQueueOptions);

		_commandHandlerAsync = commandHandlerAsync;

		var serviceBusClient = new ServiceBusClient(azureQueueOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureQueueOptions.QueueName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureQueueOptions.QueueName);
	}

	internal AzureCommandProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		var azureQueueOptions = new AzureQueueOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			QueueName = brokerOptions.QueueName
		};
		CreateQueueIfNotExist(azureQueueOptions);

		_messageMapper = messageMapper;

		var serviceBusClient = new ServiceBusClient(azureQueueOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureQueueOptions.QueueName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureQueueOptions.QueueName);
	}

	internal AzureCommandProcessorAsync(BrokerOptions brokerOptions)
	{
		var azureQueueOptions = new AzureQueueOptions
		{
			PrimaryConnectionString = brokerOptions.ConnectionString,
			QueueName = brokerOptions.QueueName
		};
		CreateQueueIfNotExist(azureQueueOptions);

		var serviceBusClient = new ServiceBusClient(azureQueueOptions.PrimaryConnectionString);
		_serviceBusProcessor =
			serviceBusClient.CreateProcessor(azureQueueOptions.QueueName);
		_serviceBusProcessor.ProcessMessageAsync += MessageProcessorHandler;
		_serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

		_serviceBusSender = serviceBusClient.CreateSender(azureQueueOptions.QueueName);
	}

	public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

	public void RegisterBroker()
	{
		if (_commandHandlerAsync == null)
			throw new Exception(
				$"No CommandHandler has found for {typeof(T)}. A CommandHandler must be specified for every Command");

		StartProcessingAsync().GetAwaiter().GetResult();
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = new())
	{
		try
		{
			// Map the message
			var command = _messageMapper != null
				? _messageMapper.MapToRequest(message)
				: JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body.Bytes));

			// Process the command
			await _commandHandlerAsync.HandleAsync(command, cancellationToken);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
		}
	}

	public async Task SendAsync(T command, CancellationToken cancellationToken = new())
	{
		try
		{
			await _serviceBusSender.SendMessageAsync(MapAthenaMessageToServiceBusMessage(command),
				cancellationToken);
		}
		catch (Exception ex)
		{
			OnException(new MufloneExceptionArgs(ex));
		}
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

	private async Task StartProcessingAsync()
	{
		await _serviceBusProcessor.StartProcessingAsync().ConfigureAwait(false);
	}

	public async Task HandleAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken = new())
	{
		if (cancellationToken.IsCancellationRequested)
			cancellationToken.ThrowIfCancellationRequested();

		try
		{
			var athenaMessage = MapAzureMessageToAthena(args.Message);
			// Map the message
			var command = _messageMapper != null
				? _messageMapper.MapToRequest(athenaMessage)
				: JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(athenaMessage.Body.Bytes));

			try
			{
				// Process command
				await _commandHandlerAsync.HandleAsync(command, cancellationToken);

				await args.CompleteMessageAsync(args.Message, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Thread.Sleep(1000);
				if (args.Message.DeliveryCount > 5)
					await args.DeadLetterMessageAsync(args.Message, null, cancellationToken).ConfigureAwait(false);
				else
					await args.AbandonMessageAsync(args.Message, null, cancellationToken).ConfigureAwait(false);

				OnException(new MufloneExceptionArgs(ex));
			}
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

	private static void CreateQueueIfNotExist(AzureQueueOptions azureQueueOptions)
	{
		using var serviceBusAdministraot = new ServiceBusAdmnistrator();
		serviceBusAdministraot.CreateQueueIfNotExistAsync(azureQueueOptions).GetAwaiter().GetResult();
	}

	protected virtual void OnException(MufloneExceptionArgs e)
	{
		var handler = MufloneExceptionHandler;
		handler?.Invoke(this, e);
	}

	#endregion
}
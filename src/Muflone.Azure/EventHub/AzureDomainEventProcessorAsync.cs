using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Devices.Client;
using Muflone.Azure.Exceptions;
using Muflone.Azure.Factories;
using Muflone.Messages;
using Muflone.Messages.Events;
using Message = Muflone.Messages.Message;

namespace Muflone.Azure.EventHub;

public class AzureDomainEventProcessorAsync<T> : IDomainEventProcessorAsync<T> where T : class, IDomainEvent
{
	private readonly DeviceClient _deviceClient;
	private readonly EventGridPublisherClient _eventGridPublisherClient;
	private readonly IDomainEventHandlerAsync<T> _eventHandlerAsync;
	private readonly IIoTMapper _ioTMapper;
	private readonly IMessageMapper<T> _messageMapper;

	internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IDomainEventHandlerAsync<T> eventHandlerAsync)
	{
		_messageMapper = messageMapper;
		_eventHandlerAsync = eventHandlerAsync;
		_ioTMapper = new IoTMapper();

		if (!string.IsNullOrEmpty(brokerOptions.SubscriptionName))
			_deviceClient = DeviceClient.CreateFromConnectionString(brokerOptions.SubscriptionName);

		if (!string.IsNullOrEmpty(brokerOptions.ConnectionString))
		{
			var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName);
			_eventGridPublisherClient =
				new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
		}
	}

	internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IIoTMapper ioTMapper,
		IDomainEventHandlerAsync<T> eventHandlerAsync)
	{
		_messageMapper = messageMapper;
		_eventHandlerAsync = eventHandlerAsync;
		_ioTMapper = ioTMapper;

		if (!string.IsNullOrEmpty(brokerOptions.SubscriptionName))
			_deviceClient = DeviceClient.CreateFromConnectionString(brokerOptions.SubscriptionName);

		if (!string.IsNullOrEmpty(brokerOptions.ConnectionString))
		{
			var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName);
			_eventGridPublisherClient =
				new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
		}
	}

	internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		_messageMapper = messageMapper;
		_ioTMapper = new IoTMapper();

		if (!string.IsNullOrEmpty(brokerOptions.SubscriptionName))
			_deviceClient = DeviceClient.CreateFromConnectionString(brokerOptions.SubscriptionName);

		if (!string.IsNullOrEmpty(brokerOptions.ConnectionString))
		{
			var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName);
			_eventGridPublisherClient =
				new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
		}
	}

	internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions)
	{
		_ioTMapper = new IoTMapper();

		if (!string.IsNullOrEmpty(brokerOptions.SubscriptionName))
			_deviceClient = DeviceClient.CreateFromConnectionString(brokerOptions.SubscriptionName);

		if (!string.IsNullOrEmpty(brokerOptions.ConnectionString))
		{
			var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName);
			_eventGridPublisherClient =
				new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
		}
	}

	internal AzureDomainEventProcessorAsync(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IIoTMapper ioTMapper)
	{
		_messageMapper = messageMapper;
		_ioTMapper = ioTMapper;

		if (!string.IsNullOrEmpty(brokerOptions.SubscriptionName))
			_deviceClient = DeviceClient.CreateFromConnectionString(brokerOptions.SubscriptionName);

		if (!string.IsNullOrEmpty(brokerOptions.ConnectionString))
		{
			var azureKeyCredentials = new AzureKeyCredential(brokerOptions.SubscriptionName);
			_eventGridPublisherClient =
				new EventGridPublisherClient(new Uri(brokerOptions.ConnectionString), azureKeyCredentials);
		}
	}

	public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

	public void RegisterBroker()
	{
	}

	public Task HandleAsync(Message message, CancellationToken token = new())
	{
		return Task.CompletedTask;
	}

	public async Task PublishAsync(T domainEvent, CancellationToken token = new())
	{
		try
		{
			var athenaMessage = _messageMapper.MapToMessage(domainEvent);
			var eventMessage = _ioTMapper.MapToAzure(athenaMessage);
			await _deviceClient.SendEventAsync(eventMessage, token).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			OnExceptionHandler(new MufloneExceptionArgs(ex));
		}
	}

	#region Helpers

	protected virtual void OnExceptionHandler(MufloneExceptionArgs e)
	{
		var handler = MufloneExceptionHandler;
		handler?.Invoke(this, e);
	}

	#endregion
}
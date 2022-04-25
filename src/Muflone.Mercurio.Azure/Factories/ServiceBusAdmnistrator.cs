using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Muflone.Mercurio.Azure.Factories;

public class ServiceBusAdmnistrator : IDisposable
{
    public async Task CreateTopicIfNotExistAsync(AzureTopicOptions azureTopicOptions)
    {
        var adminClient = new ServiceBusAdministrationClient(azureTopicOptions.PrimaryConnectionString);
        var topicExists = await adminClient.TopicExistsAsync(azureTopicOptions.TopicName);

        if (!topicExists)
        {
            var options = new CreateTopicOptions(azureTopicOptions.TopicName)
            {
                MaxSizeInMegabytes = 1024
            };
            await adminClient.CreateTopicAsync(options);
        }

        var subscriptionExists = await adminClient.SubscriptionExistsAsync(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName);
        if (!subscriptionExists)
        {
            var options = new CreateSubscriptionOptions(azureTopicOptions.TopicName, azureTopicOptions.SubscriptionName)
            {
                DefaultMessageTimeToLive = new TimeSpan(14, 0, 0, 0),
                DeadLetteringOnMessageExpiration = true,
                EnableDeadLetteringOnFilterEvaluationExceptions = true
            };
            await adminClient.CreateSubscriptionAsync(options);
        }
    }

    public async Task CreateQueueIfNotExistAsync(AzureQueueOptions azureQueueOptions)
    {
        var adminClient = new ServiceBusAdministrationClient(azureQueueOptions.PrimaryConnectionString);
        var queueExists = await adminClient.QueueExistsAsync(azureQueueOptions.QueueName);
            
        if (!queueExists)
        {
            var options = new CreateQueueOptions(azureQueueOptions.QueueName)
            {
                MaxDeliveryCount = 10,
                DeadLetteringOnMessageExpiration = true
            };
            await adminClient.CreateQueueAsync(options);
        }
    }

    #region Dispose
    private bool _disposed; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ServiceBusAdministrator() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}
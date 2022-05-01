using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Muflone.Messages;
using Muflone.Messages.Events;
using Newtonsoft.Json;

namespace Muflone.Azure.IotHub;

public class EventMessageHandlerAsync<T> where T : class, IDomainEvent
{
    public int MaxBatchSize { get; set; }

    private readonly IMessageMapper<T> _messageMapper;
    private readonly IIoTMapper _ioTMapper;
    private readonly IDomainEventHandlerAsync<T> _eventHandlerAsync;

    internal EventMessageHandlerAsync(IDomainEventHandlerAsync<T> eventHandlerAsync,
        IMessageMapper<T> messageMapper, IIoTMapper ioTMapper)
    {
        _eventHandlerAsync = eventHandlerAsync;
        _messageMapper = messageMapper;
        _ioTMapper = ioTMapper;
    }

    public async Task ProcessEventsAsync(IEnumerable<EventData> events)
    {
        foreach (var @event in events)
        {
            var eventArray = @event.Body.ToArray();
            if (eventArray.Length.Equals(0))
                continue;

            var domainEvent = _messageMapper != null
                ? _messageMapper.MapToRequest(_ioTMapper.MapToAthena(@event))
                : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(_ioTMapper.MapToAthena(@event).Body.Bytes));

            await _eventHandlerAsync.HandleAsync(domainEvent);
        }
    }
}
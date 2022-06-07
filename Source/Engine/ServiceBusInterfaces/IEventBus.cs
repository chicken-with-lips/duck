namespace Duck.ServiceBus;

public interface IEventBus : IServiceBus
{
    public void Emit();
    public void Enqueue(IEvent ev);
    public void AddListener<TEventType>(EventListener<TEventType> listener) where TEventType : IEvent;
    public void RemoveListener<TEventType>(EventListener<TEventType> listener) where TEventType : IEvent;
}

public delegate void EventListener<in TEventType>(TEventType ev) where TEventType : IEvent;

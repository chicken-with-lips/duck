namespace Duck.ServiceBus;

public interface IEventBus : IServiceBus
{
    public void Emit();
    public void Enqueue(IEvent ev);
    public void AddListener<TE>(EventListener<TE> listener) where TE : IEvent;
    public void RemoveListener<TE>(EventListener<TE> listener) where TE : IEvent;
}

public delegate void EventListener<in TE>(TE ev) where TE : IEvent;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using Duck.Contracts;
using Duck.Contracts.ServiceBus;
using Duck.Ecs;
using Duck.Ecs.Events;
using MessagePack.Formatters;

namespace Duck.ServiceBus
{
    public class EventBus : IEventBus
    {
        private readonly ConcurrentQueue<IEvent> _pending = new();
        private readonly ConcurrentDictionary<Type, ListenerCollectionBase> _listeners = new();

        public void Enqueue(IEvent ev)
        {
            _pending.Enqueue(ev);
        }

        public void Emit()
        {
            var events = _pending.ToArray();
            _pending.Clear();

            foreach (var e in events) {
                if (_listeners.TryGetValue(e.GetType(), out var listeners)) {
                    listeners.Emit(e);
                }
            }
        }

        public void AddListener<TE>(EventListener<TE> listener) where TE : IEvent
        {
            _listeners.TryAdd(typeof(TE), new ListenerCollection<TE>());

            var collection = _listeners[typeof(TE)] as ListenerCollection<TE>;
            collection?.Add(listener);
        }

        public void RemoveListener<TE>(EventListener<TE> listener) where TE : IEvent
        {
            if (_listeners.TryGetValue(typeof(TE), out var baseCollection)) {
                var collection = baseCollection as ListenerCollection<TE>;
                collection?.Remove(listener);
            }
        }

        private abstract class ListenerCollectionBase
        {
            public abstract void Emit(IEvent @event);
        }

        private class ListenerCollection<TE> : ListenerCollectionBase where TE : IEvent
        {
            #region Properties

            public EventListener<TE>[] Listeners => _listeners.ToArray();

            #endregion

            #region Members

            private ConcurrentBag<EventListener<TE>> _listeners = new();

            #endregion

            #region Methods

            public override void Emit(IEvent @event)
            {
                var actual = (TE) @event;

                foreach (var listener in _listeners) {
                    listener.Invoke(actual);
                }
            }

            public void Add(EventListener<TE> listener)
            {
                _listeners.Add(listener);
            }

            public void Remove(EventListener<TE> listener)
            {
                _listeners = new ConcurrentBag<EventListener<TE>>(_listeners.Except(new[] {listener}));
            }

            #endregion
        }
    }
}

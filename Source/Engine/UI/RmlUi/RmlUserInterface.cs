using ChickenWithLips.RmlUi;
using Duck.Content;
using Duck.Renderer;
using Duck.Ui.Assets;

namespace Duck.Ui.RmlUi;

public class RmlUserInterface : PlatformAssetBase<UserInterface>
{
    #region Properties

    public ElementDocument Document => _document;

    internal RmlContext Context => _context;

    #endregion

    #region Members

    private readonly RmlContext _context;
    private readonly ElementDocument _document;

    private readonly Dictionary<ValueTuple<string, Action<Event>, Element>, EventListenerWrapper> _eventListeners = new();

    #endregion

    #region Methods

    internal RmlUserInterface(RmlContext context, ElementDocument document)
    {
        _context = context;
        _document = document;
    }

    ~RmlUserInterface()
    {
        // FIXME: unbind events
    }

    public void AddEventListener(Element element, string eventName, Action<Event> action)
    {
        var tuple = new ValueTuple<string, Action<Event>, Element>(eventName, action, element);

        if (_eventListeners.ContainsKey(tuple)) {
            return;
        }

        _eventListeners.Add(tuple, new EventListenerWrapper(action));

        element.AddEventListener(eventName, _eventListeners[tuple]);
    }

    #endregion

    private class EventListenerWrapper : EventListener
    {
        private readonly Action<Event> _action;

        public EventListenerWrapper(Action<Event> action)
        {
            _action = action;
        }

        public override void ProcessEvent(Event ev)
        {
            _action.Invoke(ev);
        }
    }
}

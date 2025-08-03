#nullable enable

namespace Emotion.Editor;

public class ObjectChangeEvent // Generic change, no info
{

}

public class PropertyChangedEvent(string memberName, object? oldValue, object? newValue) : ObjectChangeEvent
{
    public string MemberName { get => memberName; }

    public object? OldValue { get => oldValue; }

    public object? NewValue { get => newValue; }
}

public class ListChangedEvent(ListChangedEvent.ChangeType type, object? item, int itemIndex) : ObjectChangeEvent
{
    public ListChangedEvent.ChangeType Type { get => type; }

    public object? Item { get => item; }

    public int ItemIndex { get => itemIndex; }

    public enum ChangeType
    {
        Add,
        Delete,
        Move
    }
}


public partial class EngineEditor
{
    private struct ObjectChangeMonitor
    {
        public object? ListeningObject;
        public object ObjectListeningTo;
        public Action<ObjectChangeEvent> OnChange;
    }

    private static Dictionary<object, List<ObjectChangeMonitor>> _objModificationTracker = new Dictionary<object, List<ObjectChangeMonitor>>();

    public static void ReportChange_ObjectProperty(object obj, string memberName, object? oldValue, object? newValue, object? changedByListener = null)
    {
        SendObjectChangeEvent(obj, new PropertyChangedEvent(memberName, oldValue, newValue), changedByListener);
    }

    public static void ReportChange_NoInfo(object obj, object? changedByListener = null)
    {
        SendObjectChangeEvent(obj, new ObjectChangeEvent(), changedByListener);
    }

    public static void ReportChange_ListItemRemoved(object list, object? deletedItem, int deletedItemIndex, object? changedByListener = null)
    {
        SendObjectChangeEvent(list, new ListChangedEvent(ListChangedEvent.ChangeType.Delete, deletedItem, deletedItemIndex), changedByListener);
    }

    public static void ReportChange_ListItemAdded(object list, object? item, int itemIndex, object? changedByListener = null)
    {
        SendObjectChangeEvent(list, new ListChangedEvent(ListChangedEvent.ChangeType.Add, item, itemIndex), changedByListener);
    }

    public static void ReportChange_ListItemMoved(object list, object? item, int itemIndex, object? changedByListener = null)
    {
        SendObjectChangeEvent(list, new ListChangedEvent(ListChangedEvent.ChangeType.Move, item, itemIndex), changedByListener);
    }

    private static void SendObjectChangeEvent(object obj, ObjectChangeEvent eventObject, object? changedByListener = null)
    {
        if (!_objModificationTracker.TryGetValue(obj, out List<ObjectChangeMonitor>? list)) return;

        for (int i = 0; i < list.Count; i++)
        {
            ObjectChangeMonitor monitorItem = list[i];
            if (changedByListener != null && monitorItem.ListeningObject == changedByListener) continue;
            monitorItem.OnChange(eventObject);
        }
    }

    public static void UnregisterForObjectChanges(object listener)
    {
        bool compact = false;
        foreach (KeyValuePair<object, List<ObjectChangeMonitor>> pair in _objModificationTracker)
        {
            List<ObjectChangeMonitor> list = pair.Value;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item.ListeningObject == listener)
                    list.RemoveAt(i);
            }

            if (list.Count == 0) compact = true;
        }

        if (compact)
            _objModificationTracker.RemoveAll((_, val) => val.Count == 0);
    }

    public static void RegisterForObjectChanges(object obj, Action<ObjectChangeEvent> onChanged, object? listener = null)
    {
        ObjectChangeMonitor listenInstance = new ObjectChangeMonitor()
        {
            ListeningObject = listener,
            ObjectListeningTo = obj,
            OnChange = onChanged
        };

        if (!_objModificationTracker.TryGetValue(obj, out List<ObjectChangeMonitor>? list))
        {
            list = new List<ObjectChangeMonitor>();
            _objModificationTracker.Add(obj, list);
        }

        list.Add(listenInstance);
    }

    public static void RegisterForObjectChangesList(IEnumerable obj, Action<ObjectChangeEvent> onChanged, object? listener = null)
    {
        foreach (object? item in obj)
        {
            RegisterForObjectChanges(item, onChanged, listener);
        }
    }
}

using System.Collections.Generic;

public static class EventBus<T>
{
    private static readonly HashSet<IEventListener<T>> listeners = new();

    public static void Register(IEventListener<T> listener) => listeners.Add(listener);
    public static void Unregister(IEventListener<T> listener) => listeners.Remove(listener);

    public static void Raise(T e)
    {
        foreach (var listener in listeners)
            listener.OnEvent(e);
    }
}

public interface IEventListener<T>
{
    public void OnEvent(T e);
}

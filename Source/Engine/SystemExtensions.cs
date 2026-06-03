namespace Duck;

public static class SystemExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source) {
            action(item);
        }
    }
}

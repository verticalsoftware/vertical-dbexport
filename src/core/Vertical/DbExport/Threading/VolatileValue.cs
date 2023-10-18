namespace Vertical.DbExport.Threading;

public sealed class VolatileValue<T> where T : struct
{
    private T _value;
    private readonly object _syncRoot = new();

    public VolatileValue(T value) => _value = value;

    public T Value
    {
        get
        {
            lock (_syncRoot)
            {
                var value = _value;
                return value;
            }
        }
    }

    public T Exchange(Func<T, T> setter)
    {
        lock (_syncRoot)
        {
            var previous = _value;
            _value = setter(previous);
            return previous;
        }
    }
}
namespace Binarity;

public class BinarityDeserializer
{
    private Stream? _inStream = null;
    public Stream? InStream
    {
        get => _inStream;
        set => _inStream = value;
    }

    public BinarityDeserializer(Stream inStream)
    {
        _inStream = inStream;
    }

    public void Prepare()
    {
        // TODO: nothing
    }

    public T? Deserialize<T>()
        where T : new()
    {
        return default(T);
    }
}
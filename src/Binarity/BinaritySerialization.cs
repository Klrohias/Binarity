namespace Binarity;

public class BinaritySerialization
{
    public static byte[] Serialize<T>(T obj)
    {
        var stream = new MemoryStream();
        var serializer = new BinaritySerializer(stream);

        serializer.Prepare();
        serializer.Serialize(obj);

        return stream.ToArray();
    }

    public static T? Deserialize<T>(byte[] obj)
        where T : new()
    {
        var stream = new MemoryStream();
        stream.Write(new ReadOnlySpan<byte>(obj));
        stream.Seek(0, SeekOrigin.Begin);

        var deserializer = new BinarityDeserializer(stream);
        var finalObj = deserializer.Deserialize(typeof(T));
        return (T?)finalObj;
    }
}
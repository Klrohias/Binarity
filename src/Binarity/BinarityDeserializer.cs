using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Binarity.Atributes;
using BindingFlags = System.Reflection.BindingFlags;

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

    private Span<byte> ReadCompressedIntByte<T>()
        where T : struct
    {
        var typeSize = Marshal.SizeOf<T>();
        Span<byte> buffer = new byte[typeSize];
        _inStream.Read(buffer);
        return buffer;
    }

    public object? Deserialize(Type? inputType)
    {
        var objectType = (BinarityObjectType)_inStream.ReadByte();
        switch (objectType)
        {
            case BinarityObjectType.String:
            {
                // read length
                var stringLength = BitConverter.ToInt32(ReadCompressedIntByte<int>());
                Span<byte> buffer = new byte[stringLength];

                // read string
                _inStream.Read(buffer);

                return Encoding.Default.GetString(buffer);
            }
            case BinarityObjectType.Blob:
            {
                var stream = new MemoryStream();
                var blobSize = BitConverter.ToUInt64(ReadCompressedIntByte<ulong>());
                Span<byte> buffer = new byte[1024 * 1024];
                var transferMBs = blobSize / (1024 * 1024);
                for (ulong i = 0; i < transferMBs; i++)
                {
                    _inStream.Read(buffer);
                    stream.Write(buffer);
                }

                buffer = new byte[blobSize % (1024 * 1024)];
                _inStream.Read(buffer);
                stream.Write(buffer);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            case BinarityObjectType.Int8:
            {
                return ReadCompressedIntByte<sbyte>()[0];
            }
            case BinarityObjectType.Int16:
            {
                return BitConverter.ToInt16(ReadCompressedIntByte<short>());
            }
            case BinarityObjectType.Int32:
            {
                return BitConverter.ToInt32(ReadCompressedIntByte<int>());
            }
            case BinarityObjectType.Int64:
            {
                return BitConverter.ToInt64(ReadCompressedIntByte<long>());
            }
            case BinarityObjectType.UInt8:
            {
                return ReadCompressedIntByte<byte>()[0];
            }
            case BinarityObjectType.UInt16:
            {
                return BitConverter.ToUInt16(ReadCompressedIntByte<ushort>());
            }
            case BinarityObjectType.UInt32:
            {
                return BitConverter.ToUInt32(ReadCompressedIntByte<uint>());
            }
            case BinarityObjectType.UInt64:
            {
                return BitConverter.ToUInt64(ReadCompressedIntByte<ulong>());
            }
            case BinarityObjectType.Null:
            {
                return null;
            }
            case BinarityObjectType.Object:
            {
                var finalObject = Activator.CreateInstance(inputType);
                var childCount = BitConverter.ToInt32(ReadCompressedIntByte<int>());

                // load type members
                var fields = inputType.GetMembers()
                    .Where(x => x.GetCustomAttribute<BinartiryFieldAttribute>() != null)
                    .ToDictionary(x => x.GetCustomAttribute<BinartiryFieldAttribute>()!.Name);

                // read children
                var resultsDict = new Dictionary<string, object?>();

                Span<byte> nameLengthBuffer = new byte[2];
                for (int i = 0; i < childCount; i++)
                {
                    // read name
                    _inStream.Read(nameLengthBuffer);
                    var nameLength = BitConverter.ToUInt16(nameLengthBuffer);
                    Span<byte> nameBuffer = new byte[nameLength];
                    _inStream.Read(nameBuffer);
                    var name = Encoding.Default.GetString(nameBuffer);

                    // get type
                    Type childType;
                    if (!fields.ContainsKey(name))
                    {
                        childType = typeof(object);
                    }
                    else
                    {
                        var member = fields[name];
                        switch (member.MemberType)
                        {
                            case MemberTypes.Field:
                            {
                                childType = ((FieldInfo) member).FieldType;
                                break;
                            }
                            case MemberTypes.Property:
                            {
                                childType = ((PropertyInfo) member).PropertyType;
                                break;
                            }
                            default:
                            {
                                childType = typeof(object);
                                break;
                            }
                        }
                    }

                    resultsDict[name] = Deserialize(childType);
                }

                if (!fields.Any()) return finalObject;

                // map to finalObject
                foreach (var field in fields)
                {
                    if (!resultsDict.ContainsKey(field.Key)) continue;
                    switch (field.Value.MemberType)
                    {
                        case MemberTypes.Field:
                        {
                            ((FieldInfo)field.Value)!
                                .SetValue(finalObject, resultsDict[field.Key]);
                            break;
                        }
                        case MemberTypes.Property:
                        {
                            ((PropertyInfo) field.Value)!
                                .SetValue(finalObject, resultsDict[field.Key]);
                            break;
                        }
                    }
                }

                return finalObject;
            }
        }
        return null;
    }
}
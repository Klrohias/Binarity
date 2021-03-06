
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Binarity;

using System.Reflection;
using Binarity.Atributes;

public class BinaritySerializer
{
    private Stream? _outStream;
    private StreamWriter? _writer;
    public Stream? OutStream
    {
        get => _outStream;
        set => _outStream = value;
    }

    public BinaritySerializer()
    {
        
    }

    public BinaritySerializer(Stream stream)
    {
        _outStream = stream;
        CompressedInt(ulong.MaxValue);
    }

    public void Prepare()
    {
        _writer = new StreamWriter(_outStream);
        _writer.AutoFlush = true;
    }

    private byte ByteRightMove(byte input, int move)
    {
        if (move < 0)
        {
            return (byte)(input << Math.Abs(move));
        }
        return (byte)(input >> move);
    }
    private Span<byte> CompressedInt<T>(T intVal) where T : struct
    {
        var rawBytes = new Span<byte>((byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { typeof(T) })!
            .Invoke(null, new object[] { intVal })!);
        return rawBytes;
    }

    public void Serialize<T>(T? obj)
    {
        if (obj is string stringObj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.String); // object type
            Span<byte> bytesOfString = Encoding.Default.GetBytes(stringObj);
            _outStream.Write(CompressedInt(bytesOfString.Length));
            _outStream.Write(bytesOfString);
        }
        else if (obj is null)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Null); // object type
        }
        else if (obj is sbyte int8Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Int8); // object type
            _outStream.Write(CompressedInt(int8Obj));
        }
        else if (obj is short int16Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Int16); // object type
            _outStream.Write(CompressedInt(int16Obj));
        }
        else if (obj is int int32Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Int32); // object type
            _outStream.Write(CompressedInt(int32Obj));
        }
        else if (obj is long int64Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Int64); // object type
            _outStream.Write(CompressedInt(int64Obj));
        }
        else if (obj is byte uint8Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.UInt8); // object type
            _outStream.Write(CompressedInt(uint8Obj));
        }
        else if (obj is ushort uint16Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.UInt16); // object type
            _outStream.Write(CompressedInt(uint16Obj));
        }
        else if (obj is uint uint32Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.UInt32); // object type
            _outStream.Write(CompressedInt(uint32Obj));
        }
        else if (obj is ulong uint64Obj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.UInt64); // object type
            _outStream.Write(CompressedInt(uint64Obj));
        }
        else if (obj is float floatObj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Float); // object type
            _outStream.Write(BitConverter.GetBytes(floatObj));
        }
        else if (obj is Stream blobObjStream)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Blob); // object type
            _outStream.Write(CompressedInt((ulong)blobObjStream.Length));
            Span<byte> blobBuffer = new byte[1024 * 1024]; // 1 MB
            var transferMBs = blobObjStream.Length / (1024 * 1024);
            for (long i = 0; i < transferMBs; i ++)
            {
                blobObjStream.Read(blobBuffer);
                _outStream.Write(blobBuffer);
            }

            var lastData = blobObjStream.Length % (1024 * 1024);
            blobBuffer = new byte[lastData];
            blobObjStream.Read(blobBuffer);
            _outStream.Write(blobBuffer);
        }
        else if (obj is Array arrayObj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Array); // object type
            _outStream.Write(CompressedInt(arrayObj.Length));
            foreach (var element in arrayObj)
            {
                Serialize(element);
            }
        }
        else if (obj is IList listObj)
        {
            _outStream.WriteByte((byte)BinarityObjectType.Array); // object type
            _outStream.Write(CompressedInt(listObj.Count));
            foreach (var element in listObj)
            {
                Serialize(element);
            }
        }
        else
        {
            var type = obj.GetType();
            var members = type.GetMembers();
            var serializeMembers
                = members.Where(x => x.GetCustomAttribute<BinarityFieldAttribute>() != null);

            _outStream.WriteByte((byte) BinarityObjectType.Object); // object type
            _outStream.Write(CompressedInt((uint)serializeMembers.Count())); // children count
            
            foreach (var member in serializeMembers)
            {
                var fieldName = member.GetCustomAttribute<BinarityFieldAttribute>()!.Name;
                Span<byte> bytesOfFieldName = Encoding.Default.GetBytes(fieldName);
                _outStream.Write(BitConverter.GetBytes((ushort)bytesOfFieldName.Length));
                _outStream.Write(bytesOfFieldName);

                object? childObj = null;
                if (member.MemberType == MemberTypes.Property)
                {
                    childObj = type.GetProperty(member.Name).GetValue(obj);
                } else if (member.MemberType == MemberTypes.Field)
                {
                    childObj = type.GetField(member.Name).GetValue(obj);
                }

                Serialize(childObj);
            }
        } 
        _outStream.Flush();
    }
}
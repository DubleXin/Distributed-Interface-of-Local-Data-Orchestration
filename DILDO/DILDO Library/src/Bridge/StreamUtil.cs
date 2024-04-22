using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011 
public static class StreamUtil
{
    
    [Serializable]
    public class Packet
    {
        public required string TypeName { get; set; }
        public required object ObjectData { get; set; }

        public static byte[] Serialize(Packet obj)
        {
            BinaryFormatter bf = new();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Packet Deserialize(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (Packet)binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
    private static void WriteBytes(NetworkStream stream, byte[] bytes)
    {
        stream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
        stream.Write(bytes, 0, bytes.Length);
    }
    public static void Write(NetworkStream stream, object obj)
    {
        var packet = new Packet
        {
            TypeName = obj.GetType().Name,
            ObjectData = obj
        };

        WriteBytes(stream, Packet.Serialize(packet));
    }
    public static void Write(NetworkStream stream, Packet packet)
    {
        WriteBytes(stream, Packet.Serialize(packet));
    }
    private static byte[]? ReadBytes(NetworkStream stream)
    {
        int byteCountToRead = BitConverter.ToInt32(Read(stream, 4), 0);
        return Read(stream, byteCountToRead);
    }

    public static Packet? Read(NetworkStream stream)
    {
        byte[] bytes = ReadBytes(stream);
        if (bytes == null || bytes.Length <= 4)
            return null;

        return Packet.Deserialize(bytes);
    }

    private static byte[]? Read(NetworkStream pStream, int pByteCount)
    {
        byte[] bytes = new byte[pByteCount];
        int bytesRead = 0;
        int totalBytesRead = 0;

        try
        {
            while (totalBytesRead != pByteCount &&
                    (bytesRead = pStream.Read(bytes, totalBytesRead, pByteCount - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }
        }
        catch { }

        return (totalBytesRead == pByteCount) ? bytes : null;
    }
}





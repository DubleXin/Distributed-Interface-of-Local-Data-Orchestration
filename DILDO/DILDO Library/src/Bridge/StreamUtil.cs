using System.Net.Sockets;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class StreamUtil
{
    
    [Serializable]
    public class ClassPacket
    {
        public required string ClassName { get; set; }
        public required object ClassData { get; set; }

        public static ClassPacket Deserialize(byte[] byteArray)
        {
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ClassPacket));
                return (ClassPacket)serializer.ReadObject(memoryStream);
            }
        }
    }

    private static DataContractSerializer _serializer = new(typeof(ClassPacket));
    public static void Write(NetworkStream pStream, byte[] pBytes)
    {
        pStream.Write(BitConverter.GetBytes(pBytes.Length), 0, 4);
        pStream.Write(pBytes, 0, pBytes.Length);
    }
    public static void Write(NetworkStream pstream, object obj) => WriteObject(obj, pstream);
    private static void WriteObject(object obj, NetworkStream stream)
    {
        var classPacket = new ClassPacket
        {
            ClassName = obj.GetType().Name,
            ClassData = obj
        };

        _serializer.WriteObject(stream, classPacket);
    }
    public static byte[]? Read(NetworkStream pStream)
    {
        int byteCountToRead = BitConverter.ToInt32(Read(pStream, 4), 0);
        return Read(pStream, byteCountToRead);
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





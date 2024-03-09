using DILDO.server.models;
using DILDO.net.IO;

using System.Text;

namespace DILDO.server.core.factories
{
    /// <summary>
    /// Make sure when building packet
    /// you are using pattern
    /// msgID[0] -> OpCode(for broadcast only)[1] -> Data(any size)[-] ->confirmID[^1]
    /// </summary>
    public class PacketBroadcast : IPacketFactory
    {
        public static readonly OpCode OPCODE = OpCode.BroadcastStringMessage;
        public byte[]? BuildData(object msg)
        {
            var builder = new PacketBuilderBroker();
            string[] encoded = msg as string[];
            if (encoded is null)
                throw new NullReferenceException();
            if (encoded.Length < 4)
                throw new Exception("The brodcas msg length was less then 4. Read summary!");
            var dataToSend = new StringBuilder();
            dataToSend
                .Append("|").Append(encoded[0])
                .Append("|").Append(encoded[1])
                .Append("|");
            for (int i = 2; i < encoded.Length - 1; i++)
                dataToSend.Append(encoded[i]).Append("|");
            dataToSend.Append(encoded[^1]);
            builder.WriteMessage(dataToSend.ToString());
            return builder.GetPacketBytes();
        }

        public Packet GetPacket(object data) =>
            new Packet()
            {
                OpCode = OPCODE,
                Data = BuildData(data)
            };
    }
}

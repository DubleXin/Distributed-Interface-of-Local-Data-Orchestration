using DILDO.server.models;
using DILDO.net.IO;

namespace DILDO.server.core.factories
{
    public class PacketDisconnect : IPacketFactory
    {
        public static readonly OpCode OPCODE = OpCode.DisconnectMessage;
        public byte[]? BuildData(object data)
        {
            var builder = new PacketBuilderBroker();
            builder.WriteOPCode((byte)OPCODE);
            string? encoded = data as string;
            if (encoded is null)
                throw new NullReferenceException();
            builder.WriteMessage(encoded);
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

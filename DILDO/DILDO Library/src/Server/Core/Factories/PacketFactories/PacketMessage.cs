using DILDO.server.models.connectors;
using DILDO.net.IO;

namespace DILDO.server.core.factories
{
    public class PacketMessage : IPacketFactory
    {
        public static readonly OpCode OPCODE = OpCode.StringMessage;
        public byte[]? BuildData(object data)
        {
            var builder = new PacketBuilderBroker();
            builder.WriteOPCode((byte)OPCODE);
            string[]? encoded = data as string[];
            if (encoded is null)
                throw new NullReferenceException();
            var message = encoded[0];
            var guid = encoded[1];
            builder.WriteMessage(message);
            builder.WriteMessage(guid);
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

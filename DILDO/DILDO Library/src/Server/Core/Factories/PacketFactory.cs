using DILDO.server.models.connectors;

namespace DILDO.server.core.factories
{
    public static class PacketFactory
    {
        private static readonly Dictionary<OpCode, IPacketFactory> _factories = new()
        {
            {OpCode.StringMessage, new PacketMessage()},
            {OpCode.DisconnectMessage, new PacketDisconnect()},
            {OpCode.BroadcastStringMessage, new PacketBroadcast()}
        };
        public static IPacketFactory GetFactory(OpCode code)
        {
            if (!_factories.TryGetValue(code, out var factory))
                throw new Exception($"Invalid opcode | opcode:{code}");
            return factory;
        }
    }
}

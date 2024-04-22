using DILDO.server.models.connectors;

namespace DILDO.server.core.factories
{
    public interface IPacketFactory
    {
        Packet GetPacket(object data);
        byte[]? BuildData(object data);
    }
}

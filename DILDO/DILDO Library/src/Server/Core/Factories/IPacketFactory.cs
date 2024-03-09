using DILDO.server.models;

namespace DILDO.server.core.factories
{
    public interface IPacketFactory
    {
        Packet GetPacket(object data);
        byte[]? BuildData(object data);
    }
}

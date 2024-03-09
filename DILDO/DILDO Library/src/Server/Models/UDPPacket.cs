using System.Text;
using static DILDO.net.IO.PacketReaderBroker;

namespace DILDO.server.models
{
    public struct UDPPacket
    {
        public (DataType type, string data)[] Data;
        public Guid ID;
        public Guid ConfirmID;
        public int OpCode;
        public string[] RawData;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Data)
                stringBuilder.Append(item.data).Append("; ");
            return stringBuilder.ToString();
        }
    }
}

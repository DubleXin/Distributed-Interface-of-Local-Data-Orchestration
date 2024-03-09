using System.Text;

namespace DILDO.net.IO
{
    public class PacketBuilderBroker
    {
        private MemoryStream _ms;
        public PacketBuilderBroker() => _ms = new();
        public void WriteOPCode(byte opCode) => _ms.WriteByte(opCode);
        public void WriteMessage(string message)
        {
            var bitMesage = Encoding.UTF32.GetBytes(message);
            var length = bitMesage.Length;
            _ms.Write(BitConverter.GetBytes(length));
            _ms.Write(bitMesage);
        }
        public byte[] GetPacketBytes() => _ms.ToArray();
    }
}

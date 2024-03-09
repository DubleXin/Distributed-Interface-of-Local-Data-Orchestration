namespace DILDO.net.IO
{
    public class PacketReaderBroker
    {
        public enum DataType
        {
            ConfirmID,
            PacketID,
            ActualData,
            OperationCode
        }
        public const char PACKET_SEPARATOR = '|';

        private string _message;
        private List<(DataType type, string data)> _packetContent;
        private List<string> _packetRawContent;
        public PacketReaderBroker(string message)
        {
            _message = message;
            _packetContent = new();
            _packetRawContent = new();
            ReadContent();
        }

        private void ReadContent()
        {
            string[] buffer = _message.Split(PACKET_SEPARATOR);
            _packetContent.Add(( type: DataType.PacketID, data: buffer[1]));
            _packetRawContent.Add(buffer[1]);
            _packetContent.Add(( type: DataType.OperationCode, data: buffer[2]));
            _packetRawContent.Add(buffer[2]);
            _packetContent.Add(( type: DataType.ConfirmID, data: buffer[^1]));
            if (buffer.Length < 5)
                return;
            for (int i = 3; i < buffer.Length; i++)
            {
                if(i < buffer.Length - 1)
                    _packetContent.Add((type: DataType.ActualData, data: buffer[i]));
                _packetRawContent.Add(buffer[i]);
            }
        }

        public Guid GetID(DataType type)
        {
            if(type == DataType.ConfirmID || type == DataType.PacketID)
                foreach (var packet in _packetContent)
                {
                    if (type == DataType.PacketID && packet.type == DataType.PacketID)
                        return Guid.Parse(packet.data);
                    if (type == DataType.ConfirmID && packet.type == DataType.ConfirmID)
                        return Guid.Parse(packet.data);
                }
            return Guid.Empty;
        }

        public (DataType type, string data)[] GetPacketData() => _packetContent.ToArray();
        public string[] GetRawPacketData() => _packetRawContent.ToArray();

        internal int GetOpCode()
        {
            foreach (var packet in _packetContent)
                if (packet.type == DataType.OperationCode)
                    return int.Parse(packet.data);
            return -1;
        }
    }
}

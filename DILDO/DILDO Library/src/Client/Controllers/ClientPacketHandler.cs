using DILDO.client;
using DILDO.controllers;
using DILDO.net.IO;
using DILDO.server.models;
using System.Net;
using System.Text;

namespace Client.Controllers
{
    public class ClientPacketHandler : PacketHandler
    {
        private ClientState _owner;
        public ClientPacketHandler(ClientState owner) : base() => _owner = owner; 
        protected override void Communication()
        {
            
        }

        protected override void Pairing()
        {
            _owner.Model.ReceiveClient.EnableBroadcast = true;
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                byte[] message = _owner.Model.ReceiveClient.Receive(ref endpoint);
                string encoded = Encoding.UTF32.GetString(message);

                PacketReaderBroker reader = new(encoded);

                var packet = new UDPPacket()
                {
                    ID = reader.GetID(PacketReaderBroker.DataType.PacketID),
                    ConfirmID = reader.GetID(PacketReaderBroker.DataType.ConfirmID),
                    OpCode = reader.GetOpCode(),
                    Data = reader.GetPacketData(),
                    RawData = reader.GetRawPacketData(),
                };

                if (packet.RawData.Length < 4)
                    return;

                TryAddPacketToList(packet);
                TryAddServerToList(packet);
            }
            catch { }
        }

        private void TryAddServerToList(UDPPacket packet)
        {
            var raw = packet.RawData;
            if (Guid.TryParse(raw[2], out var guid))
                if (_owner.Model.ServerNames.TryAdd(guid, raw[3]) && _owner.Model.ServerConnectInfo.TryAdd(guid, packet.ConfirmID))
                {
                    var servers = _owner.Model.ServerNames.ToArray();
                    List<(Guid, string)> listToInvoke = new();
                    foreach (var server in servers)
                        listToInvoke.Add((server.Key, server.Value));
                    _owner.Model.OnServerAddressFound?.Invoke(listToInvoke.ToArray());
                }
        }
        private void TryAddPacketToList(UDPPacket packet)
        {
            if (ReceivedPacketBuffer.TryAdd(packet.ID, packet))
                _owner.RaisePacketReceiver(packet, "private void TryAddPacketToList(UDPPacket packet)");
        }
    }
}

using DILDO;
using DILDO.client;
using DILDO.client.models;
using DILDO.controllers;
using DILDO.net.IO;
using DILDO.server.models;
using System.Net;
using System.Text;

namespace Client.Controllers
{
    public class ClientPacketHandler : PacketHandler
    {
        public ClientPacketHandler() : base() { }

        protected override void LifeCycle()
        {
            ClientState.Instance.Model.ReceiveClient.EnableBroadcast = true;
            base.LifeCycle();
        }

        protected override void Pairing()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                byte[] message = ClientState.Instance.Model.ReceiveClient.Receive(ref endpoint);
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

                TryAddUDPPacketToList(packet);
                TryAddServerToList(packet);
            }
            catch(Exception ex) { Debug.Log($"<RED>{ex.Message}"); }
        }
        protected override void Communication()
        {

        }

        private void TryAddServerToList(UDPPacket packet)
        {
            var model = ClientState.Instance.Model;
            var raw = packet.RawData;

            if (Guid.TryParse(raw[2], out var guid))
            {
                var v4 = raw[4] == "" ? null : new IPEndPoint(IPAddress.Parse(raw[4]), ushort.Parse(raw[6]));
                var v6 = raw[5] == "" ? null : new IPEndPoint(IPAddress.Parse(raw[5]), ushort.Parse(raw[6]));

                if(v4 is null && v6 is null)
                {
                    Debug.Exception("TryAddServerToList(UDPPacket packet) NullReferenceException", "IPv4 and IPv6 were nulls");
                    return;
                }

                var serverData = new ServerData(raw[3], Guid.Parse(raw[2]), v4, v6);
                if (model.Servers.TryAdd(guid, serverData))
                {
                    var servers = model.Servers.ToArray();

                    List<(Guid, string)> listToInvoke = new();
                    foreach (var server in servers)
                        listToInvoke.Add((server.Key, server.Value.ServerName));
                    
                    model.OnServerAddressFound?.Invoke(listToInvoke.ToArray());
                }
            }
        }
        private void TryAddUDPPacketToList(UDPPacket packet)
        {
            if (ReceivedPacketBuffer.TryAdd(packet.ID, packet))
                ClientState.Instance.RaisePacketReceiver(packet, "private void TryAddPacketToList(UDPPacket packet)");
        }
    }
}

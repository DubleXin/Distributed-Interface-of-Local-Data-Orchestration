using DILDO;
using DILDO.client;
using DILDO.controllers;
using DILDO.net.IO;
using DILDO.server.models;
using DILDO.server.models.connectors;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Controllers
{
    public class ClientConnector : NetworkConnector
    {
        public UdpClient? SendClient    { get; private set; }
        public UdpClient? ReceiveClient { get; private set; }

        public ClientConnector() : base() { }

        public override void StartPairing()
        {
            Debug.Log<ClientConnector>($" <WHI>Client <DGE>started pairing.");
            base.StartPairing();
        }
        public override void StopPairing()
        {
            Debug.Log<ClientConnector>($" <WHI>Client <DGE>stopped pairing.");
            base.StartPairing();
        }

        protected override void LifeCycle()
        {
            ReceiveClient.EnableBroadcast = true;
            base.LifeCycle();
        }

        protected override void Pairing()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                byte[] message = ReceiveClient.Receive(ref endpoint);
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

                TryAddServerToList(packet);
            }
            catch(Exception ex) { Debug.Log($"<RED>{ex.Message}"); }
        }
        private void TryAddServerToList(UDPPacket packet)
        {
            var model = ClientState.Instance.Data;
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

                var serverData = new DILDO.client.models.ServerData(raw[3], Guid.Parse(raw[2]), v4, v6);
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

        public override void Launch()
        {
            SendClient = new UdpClient();
            ReceiveClient = new UdpClient(ServerData.DEFAULT_SERVER_SEND_PORT);

            base.Launch();
        }
        public override void Close()
        {
            base.Close();

            SendClient.Close();
            ReceiveClient.Close();
        }
    }
}

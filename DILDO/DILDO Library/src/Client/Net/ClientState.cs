using DILDO.server.models;
using DILDO.net.IO;
using DILDO.server.core.factories;
using static DILDO.server.controllers.ServerPacketHandler;

using System.Net;
using System.Text;

using ServerModel = DILDO.server.models.ServerModel;
using DILDO.controllers;
using DILDO.server.controllers;
namespace DILDO.client;

public class ClientState : StateProfile, IDisposable
{
    public static ClientState? Instance { get; private set; }

    public override PacketHandler? PacketHandler
    {
        get => _packetHandler;
        protected set => _packetHandler = value as ServerPacketHandler;
    }
    private ServerPacketHandler? _packetHandler; //TODO CLIENT

    public ClientModel Model { get; private set; }

    public ClientState() : base()
    {
        Model = new();
        Instance = this;
    }

    public (Guid, string)[] GetServers()
    {
        var servers = Model.ServerNames.ToArray();
        List<(Guid, string)> listToInvoke = new();
        foreach (var server in servers)
            listToInvoke.Add((server.Key, server.Value));
        return listToInvoke.ToArray();
    }

    public override void Launch()
    {
        Debug.Log<ClientState>($"<WHI> Client Started.");

        ListenToConnection();
    }

    private void ListenToConnection()
    {
        Model.ReceiveClient.EnableBroadcast = true;
        Task.Run(() =>
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while(!Model.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] message = Model.ReceiveClient.Receive(ref endpoint);
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
                        continue;

                    TryAddPacketToList(packet);
                    TryAddServerToList(packet);
                }
                catch { }
            }
            Model.CancellationToken.Dispose();
            Dispose();
        });
    }
    private void TryAddServerToList(UDPPacket packet)
    {
        var raw = packet.RawData;
        if (Guid.TryParse(raw[2], out var guid))
            if (Model.ServerNames.TryAdd(guid, raw[3]) && Model.ServerConnectInfo.TryAdd(guid, packet.ConfirmID))
            {
                var servers = Model.ServerNames.ToArray();
                List<(Guid, string)> listToInvoke = new();
                foreach (var server in servers)
                    listToInvoke.Add((server.Key, server.Value));
                Model.OnServerAddressFound?.Invoke(listToInvoke.ToArray());
            }
    }
    private void TryAddPacketToList(UDPPacket packet)
    {
        if (Model.ReceivedPackets.TryAdd(packet.ID, packet))
            RaisePacketReceiver(packet, "private void TryAddPacketToList(UDPPacket packet)");
    }
    public void ConnectToServer(Guid guid)
    {
        if(!Model.ServerConnectInfo.TryGetValue(guid, out var info))
            return;
        var sendingData = PacketFactory.GetFactory
                (OpCode.BroadcastStringMessage).GetPacket(new string[]
                {
                    Guid.NewGuid().ToString(),
                    ((int)PacketHandler.PacketType.SessionConfirm).ToString(),
                    guid.ToString(),
                    info.ToString(),
                    Model.ID.ToString(),
                    Guid.NewGuid().ToString()
                }).Data;

        if (sendingData is null)
            return;

        var endpoint = new IPEndPoint
        (IPAddress.Broadcast, ServerModel.DEFAULT_SERVER_RECEIVE_PORT);
        Model.SendClient.Send(sendingData, sendingData.Length, endpoint);
        Debug.Log<ClientState>($"<DMA> Requesting connection to server named: <MAG>{Model.ServerNames[guid]}");
    }

    public override void Close() 
    {
        Model.CancellationToken.Cancel();
        Model.ReceiveClient.Close(); 
    }

    public void Dispose()
    {
        Model.Dispose();

        Debug.Log<ClientState>("<DRE> Client Closed and Disposed.");
        StateBroker.Instance.OnStateClosed?.Invoke();
    }
}
using DILDO.server.models;
using DILDO.net.IO;
using DILDO.server.core.factories;
using static DILDO.server.controllers.PacketRouter;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

using ServerModel = DILDO.server.models.ServerModel;
using DILDO.server;

namespace DILDO.client;
public class ClientState : StateProfile, IDisposable
{
    public static ClientState? Instance { get; private set; }

    private UdpClient? _sendClient;
    private UdpClient? _receiveClient;
    public Action? OnUserConnectEvent;
    public Action? OnUserDisconnectEvent;
    public Action<(Guid, string)[]>? OnServerAddressFound;

    private ConcurrentDictionary<Guid, string>? _serverNames;
    private ConcurrentDictionary<Guid, Guid>? _serverConnectInfo;
    private ConcurrentDictionary<Guid, UDPPacket>? _receivedPackets;

    private Guid _id;

    private CancellationTokenSource? _cts;

    public ClientState() : base()
    {
        _id = Guid.NewGuid();

        Instance = this;
    }

    public (Guid, string)[] GetServers()
    {
        var servers = _serverNames.ToArray();
        List<(Guid, string)> listToInvoke = new();
        foreach (var server in servers)
            listToInvoke.Add((server.Key, server.Value));
        return listToInvoke.ToArray();
    }

    public override void Launch()
    {
        _cts = new CancellationTokenSource();
        _receiveClient = new UdpClient(ServerModel.DEFAULT_SERVER_SEND_PORT);
        _sendClient = new UdpClient();

        _serverNames = new ConcurrentDictionary<Guid, string>();
        _receivedPackets = new();
        _serverConnectInfo = new();

        Debug.Log<ClientState>($"<WHI> Client Started.");

        ListenToConnection();
    }

    private void ListenToConnection()
    {
        _receiveClient.EnableBroadcast = true;
        Task.Run(() =>
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while(!_cts.IsCancellationRequested)
            {
                try
                {
                    byte[] message = _receiveClient.Receive(ref endpoint);
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
            _cts.Dispose();
            Dispose();
        });
    }
    private void TryAddServerToList(UDPPacket packet)
    {
        var raw = packet.RawData;
        if (Guid.TryParse(raw[2], out var guid))
            if (_serverNames.TryAdd(guid, raw[3]) && _serverConnectInfo.TryAdd(guid, packet.ConfirmID))
            {
                var servers = _serverNames.ToArray();
                List<(Guid, string)> listToInvoke = new();
                foreach (var server in servers)
                    listToInvoke.Add((server.Key, server.Value));
                OnServerAddressFound?.Invoke(listToInvoke.ToArray());
            }
    }
    private void TryAddPacketToList(UDPPacket packet)
    {
        if (_receivedPackets.TryAdd(packet.ID, packet))
            RaisePacketReceiver(packet, "private void TryAddPacketToList(UDPPacket packet)");
    }
    public void ConnectToServer(Guid guid)
    {
        if(!_serverConnectInfo.TryGetValue(guid, out var info))
            return;
        var sendingData = PacketFactory.GetFactory
                (OpCode.BroadcastStringMessage).GetPacket(new string[]
                {
                    Guid.NewGuid().ToString(),
                    ((int)PacketType.SessionConfirm).ToString(),
                    guid.ToString(),
                    info.ToString(),
                    _id.ToString(),
                    Guid.NewGuid().ToString()
                }).Data;

        if (sendingData is null)
            return;

        var endpoint = new IPEndPoint
        (IPAddress.Broadcast, ServerModel.DEFAULT_SERVER_RECEIVE_PORT);
        _sendClient.Send(sendingData, sendingData.Length, endpoint);
        Debug.Log<ClientState>($"<DMA> Requesting connection to server named: <MAG>{_serverNames[guid]}");
    }

    public override void Close() 
    { 
        _cts.Cancel(); 
        _receiveClient.Close(); 
    }

    public void Dispose()
    {
        _sendClient.Close();

        _receiveClient.Dispose();
        _sendClient.Dispose();

        Debug.Log<ClientState>("<DRE> Client Closed and Disposed.");
        StateBroker.Instance.OnStateClosed?.Invoke();
    }

}


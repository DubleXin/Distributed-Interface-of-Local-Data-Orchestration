using DILDO.server.models;
using DILDO.net.IO;
using DILDO.server.core.factories;

using System.Net;

using ServerModel = DILDO.server.models.ServerModel;
using DILDO.controllers;
using Client.Controllers;
namespace DILDO.client;

public class ClientState : StateProfile, IDisposable
{
    public static ClientState? Instance { get; private set; }

    public override PacketHandler? PacketHandler
    {
        get => _packetHandler;
        protected set => _packetHandler = value as ClientPacketHandler;
    }
    private ClientPacketHandler? _packetHandler;

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
    }
    //TODO adapt to tcp
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
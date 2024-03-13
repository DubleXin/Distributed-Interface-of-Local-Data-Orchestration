using DILDO.controllers;
using Client.Controllers;
using DILDO.client.models;

namespace DILDO.client;

public class ClientState : StateProfile
{
    #region FIELDS
    
    public static ClientState? Instance { get; private set; }

    public override PacketHandler? PacketHandler
    {
        get => _packetHandler;
        protected set => _packetHandler = value as ClientPacketHandler;
    }
    private ClientPacketHandler? _packetHandler;

    public ClientModel? Model { get; private set; }

    #endregion

    #region CONSTRUCTOR

    public ClientState() : base() => Instance = this;
    
    #endregion

    #region LIFE CYCLE

    public override void Launch()
    {
        Model = new ClientModel();
        PacketHandler = new ClientPacketHandler();

        PacketHandler.OnDisposed += Model.Dispose;

        Debug.Log<ClientState>($" <WHI>Client <GRE>Started.");

        PacketHandler.Launch();
    }
    public override void Close()
    {
        Model.SendClient.Close();
        Model.ReceiveClient.Close();

        PacketHandler.LifeCycleCTS.Cancel();
    }

    #endregion

    #region CONTROLS
    
    public (Guid, string)[] GetServers()
    {
        var servers = Model.Servers.ToArray();
        List<(Guid, string)> listToInvoke = new();
        foreach (var server in servers)
            listToInvoke.Add((server.Key, server.Value.ServerName));
        return listToInvoke.ToArray();
    }

    public void ConnectToServer(Guid guid)
    {
        Debug.Log<ClientState>($"<DMA> Requesting connection to server named: <CYA>{Model.Servers[guid].ServerName}");
        if (!Model.Servers.TryGetValue(guid, out var serverData))
            return;

        Debug.Log<ClientState>($"<YEL>  " +
            $"{(serverData.V4 is null ? "" : $"IPv4 : {serverData.V4}")} , " +
            $"{(serverData.V6 is null ? "" : $"IPv6 : {serverData.V6}")}");


    }
    
    #endregion
}
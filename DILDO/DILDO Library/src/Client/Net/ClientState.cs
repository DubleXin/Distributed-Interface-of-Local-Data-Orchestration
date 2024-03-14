using DILDO.controllers;
using Client.Controllers;
using DILDO.client.models;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

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
        Model.TcpClient.Close();
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

        try
        {
            if (serverData.V4 is not null)
                Model.TcpClient.Connect(serverData.V4.Address, serverData.V4.Port);
            else if (serverData.V6 is not null)
                Model.TcpClient.Connect(serverData.V6.Address, serverData.V6.Port);

            NetworkStream stream = Model.TcpClient.GetStream();

            //RECEIVE TEST ZONE // THERE SHOULD BE VALIDATION-SPECIFIED TALKING
            //AND IF VALIDATION APPROVES THE CONNECTION WE SAVE THE SERVER
            //AND ITS NETWORK STREAM FOR "COMMUNICATION" AND SET IsCommunicating TO true.
            byte[] buffer = new byte[4096];
            stream.Read(buffer, 0, 4096);
            Debug.Log<ClientState>(Encoding.UTF32.GetString(buffer));
            stream.Read(buffer, 0, 4096);
            Debug.Log<ClientState>(Encoding.UTF32.GetString(buffer));
            stream.Read(buffer, 0, 4096);
            Debug.Log<ClientState>(Encoding.UTF32.GetString(buffer));
        }
        catch(Exception ex) { Debug.Exception(ex.Message, "no additional info."); }
    }
    
    #endregion
}
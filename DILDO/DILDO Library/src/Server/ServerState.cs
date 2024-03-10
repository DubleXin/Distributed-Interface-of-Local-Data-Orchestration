using DILDO.client.MVM.model;
using DILDO.server.config;
using DILDO.server.controllers;
using DILDO.server.models;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server;
public class ServerState  : StateProfile, IDisposable
{
    public static ServerState? Instance { get; private set; }

    private ServerModel? _server;
    private PacketHandler? _packetHandler;
    public PacketHandler? @PacketHandler { get => _packetHandler; }

    private CancellationTokenSource _cts;

    public ServerState() : base() => Instance = this;
    
    public override void Launch()
    {
        _cts = new();
        _server = new();
        _packetHandler = new(_server, this);
        Debug.Log<ServerState>($"<CYA>User [{NetworkingData.This.UserName}]<WHI> Starts Server.");
        Task.Run(Process);
    }
    private void Process()
    {
        new ServerSetup(ref _server, _packetHandler);
        while (!_cts.Token.IsCancellationRequested)
        {

        }
        _packetHandler.BroadcastCancellationToken.Cancel();
        _packetHandler.PacketHandlingCancellationToken.Cancel();
    }

    public override void Close() => _cts.Cancel();
    public void Dispose() => _server.Dispose();
}

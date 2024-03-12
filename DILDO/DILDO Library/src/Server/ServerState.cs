using DILDO.controllers;
using DILDO.server.controllers;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server;
public class ServerState : StateProfile, IDisposable
{
    public static ServerState? Instance { get; private set; }

    private ServerModel? _model;
    public ServerModel? Model { get => _model; }

    public override PacketHandler? PacketHandler 
    { 
        get => _packetHandler; 
        protected set => _packetHandler = value as ServerPacketHandler; 
    }
    private ServerPacketHandler? _packetHandler;

    private CancellationTokenSource? _cts;

    public ServerState() : base() => Instance = this;
    
    public override void Launch()
    {
        _cts = new();
        _model = new();
        _packetHandler = new();

        Debug.Log<ServerState>($"<WHI> Server Started.");
        Task.Run(LifeCycle);
    }
    private void LifeCycle()
    {
        _packetHandler.Launch();
        _model.Client.EnableBroadcast = true;

        Task.Run(() =>
        {
            while (!_model.CancellationToken.IsCancellationRequested) {}
            Debug.Log<ServerState>($"<DRE> Server Closed and Disposed.");
            StateBroker.Instance.OnStateClosed?.Invoke();
        });

        while (!_cts.Token.IsCancellationRequested) { }
        _packetHandler.LifeCycleCTS.Cancel();
    }

    public override void Close() => _cts.Cancel();
    public void Dispose() => _model.Dispose();
}

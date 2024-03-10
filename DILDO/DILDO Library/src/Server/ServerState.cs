using DILDO.server.controllers;
using DILDO.server.models;
using System.Net;
using System.Reflection;
using System.Text;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server;
public class ServerState  : StateProfile, IDisposable
{
    public static ServerState? Instance { get; private set; }

    private ServerModel? _server;
    private PacketRouter? _packetRouter;
    public PacketRouter? PacketRouter { get => _packetRouter; }

    private CancellationTokenSource _cts;

    public ServerState() : base() => Instance = this;
    
    public override void Launch()
    {
        _cts = new();
        _server = new();
        _packetRouter = new(_server, this);
        Debug.Log<ServerState>($"<WHI> Server Started.");
        Task.Run(Process);
    }
    private void Process()
    {
        _packetRouter.Broadcast();

        _server.Client.EnableBroadcast = true;
        Task.Run(() =>
        {
            Debug.Log<ServerState>($"<GRA> Server is listening.");
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (!_server.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] buffer = _server.Client.Receive(ref endpoint);
                    string encoded = Encoding.UTF32.GetString(buffer);
                    _packetRouter.InvokePacketReceive(encoded);
                }
                catch { }
            }
            Debug.Log<ServerState>($"<DRE> Server Closed and Disposed.");
            StateBroker.Instance.OnStateClosed?.Invoke();
        });
        while (!_cts.Token.IsCancellationRequested) { }
        _packetRouter.BroadcastCancellationToken.Cancel();
        _packetRouter.PacketHandlingCancellationToken.Cancel();
    }
    public override void Close() => _cts.Cancel();
    public void Dispose() => _server.Dispose();
}

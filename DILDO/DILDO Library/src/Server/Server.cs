using DILDO.client.MVM.model;
using DILDO.server.Config;
using DILDO.server.controllers;
using DILDO.server.models;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server;
public class Server  : NetProfile
{
    private User _owner;

    private ServerModel _server;
    private PacketHandler _packetHandler;
    public PacketHandler @PacketHandler { get => _packetHandler; }

    private CancellationTokenSource _cts;

    public Server(User owner) : base()
    {
        _owner = owner;

        _server = new();
        _packetHandler = new(_server, owner, this);
        _cts = new();
    }
    public override void Launch()
    {
        Debug.Log<Server>("Server Started <GRA>| <DYE>1.7.0a1<YEL> Networking Experience Requiem Edition\n");
        Task.Run(() => Process());
    }
    private void Process()
    {
        new ServerSetup(ref _server, _packetHandler);
        while (!_cts.Token.IsCancellationRequested) 
        { 
                
        }
        /*_packetHandler.Dispose();*/
        _server.Dispose();
    }
    public override void Close()
    {
        try
        {
            _cts.Cancel();
        }
        finally
        {
            _cts.Dispose();
        }
    }
}

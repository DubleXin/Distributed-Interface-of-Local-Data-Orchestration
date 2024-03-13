using DILDO.controllers;
using DILDO.server.controllers;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server;
public class ServerState : StateProfile
{
    #region FIELDS

    public static ServerState? Instance { get; private set; }

    public override PacketHandler? PacketHandler 
    { 
        get => _packetHandler; 
        protected set => _packetHandler = value as ServerPacketHandler; 
    }
    private ServerPacketHandler? _packetHandler;

    public ServerModel? Model { get; private set; }

    #endregion

    #region CONSTRUCTOR

    public ServerState() : base() => Instance = this;

    #endregion

    #region LIFE CYCLE

    public override void Launch()
    {
        Model = new();
        PacketHandler = new ServerPacketHandler();

        PacketHandler.OnDisposed += Model.Dispose;

        Debug.Log<ServerState>($" <WHI>Server <GRE>Started.");

        PacketHandler.Launch();
    }

    public override void Close()
    {
        Model.Client.Close();
        Model.Server.Close();

        PacketHandler.LifeCycleCTS.Cancel();
    }

    #endregion
}

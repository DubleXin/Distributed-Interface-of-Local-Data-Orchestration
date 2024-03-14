using DILDO.controllers;
using DILDO.server.controllers;
using DILDO.server.models;
using System.Net.Sockets;
using System.Text;

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

    public void ValidateConnection(TcpClient client)
    {
        Debug.Log<ServerState>($" <WHI>Server <MAG>Accepted connection request, starting validation sequence.");

        NetworkStream stream = client.GetStream();

        //SEND TEST ZONE // THERE SHOULD BE VALIDATION-SPECIFIC TALKING
        //AND IF WE APPROVE THE CLIENT HERE WE SEND THE APPROVAL AND SAVE
        //CLIENT AND ITS NETWORKING STREAM INTO THE DICTIONARY OF THEM 
        stream.Write(Encoding.UTF32.GetBytes("<WHI>message 1"));
        Thread.Sleep(10);
        stream.Write(Encoding.UTF32.GetBytes("<WHI>message 2"));
        Thread.Sleep(10);
        stream.Write(Encoding.UTF32.GetBytes("<WHI>message 3"));
    }

    #endregion
}

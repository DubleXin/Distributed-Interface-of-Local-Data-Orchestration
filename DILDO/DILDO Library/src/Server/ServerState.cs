using DILDO.controllers;
using DILDO.server.controllers;
using DILDO.server.models;

namespace DILDO.server;
public class ServerState : StateProfile
{
    #region FIELDS

    public static ServerState? Instance { get; private set; }

    public override NetworkConnector? Connector 
    { 
        get => _connector; 
        protected set => _connector = value as ServerConnector; 
    }
    private ServerConnector? _connector;

    public ServerData? Data { get; private set; }
    public ServerCore? Core { get; private set; }

    #endregion

    #region CONSTRUCTOR

    public ServerState() : base()
    {
        Instance = this;

        Connector = new ServerConnector();
        Core      = new ServerCore();
    }
    #endregion

    #region LIFE CYCLE

    public override void Launch()
    {
        Data = new ServerData();

        Core.Launch();
        Connector.Launch();

        Debug.Log<ServerState>($" <WHI>Server <GRE>Started.");
    }

    public override void Close()
    {
        Core.Close();
        Connector.Close();

        Debug.Log<ServerData>(" <WHI>Server<DRE> Closed.");
        StateBroker.Instance.OnStateClosed?.Invoke();
    }

    #endregion
}





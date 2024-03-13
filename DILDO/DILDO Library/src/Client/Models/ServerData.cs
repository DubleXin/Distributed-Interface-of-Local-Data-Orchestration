using System.Net;

namespace DILDO.client.models;

public class ServerData
{
    public ServerData(string? serverName, Guid serverID, IPEndPoint? v4, IPEndPoint? v6)
    {
        ServerName = serverName;
        ServerID = serverID;
        V4 = v4;
        V6 = v6;
    }

    public readonly string? ServerName;
    public readonly Guid ServerID;
    public readonly IPEndPoint? V4;
    public readonly IPEndPoint? V6;
}

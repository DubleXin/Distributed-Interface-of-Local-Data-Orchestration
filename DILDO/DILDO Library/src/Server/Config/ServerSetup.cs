using DILDO.server.controllers;
using DILDO.server.models;

using System.Net;
using System.Text;

namespace DILDO.server.config;
public class ServerSetup
{
    public ServerSetup(ref ServerModel model, PacketHandler handler)
    {
        StartListening(model, handler);
        handler.Broadcast();
    }

    private void StartListening(ServerModel model,PacketHandler handler)
    {
        model.Client.EnableBroadcast = true;
        Task.Run(() =>
        {
            Debug.Log<ServerState>($"<CYA>User [{NetworkingData.This.UserName}]<DRE> Starts listening.");
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (!model.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] buffer = model.Client.Receive(ref endpoint);
                    string encoded = Encoding.UTF32.GetString(buffer);
                    handler.InvokePacketReceive(encoded);
                }
                catch (Exception ex) { }
            }
            Debug.Log<ServerState>($"<CYA>User [{NetworkingData.This.UserName}]<DRE> Closes server.");
            StateBroker.Instance.OnStateClosed?.Invoke();
        });
    }
}

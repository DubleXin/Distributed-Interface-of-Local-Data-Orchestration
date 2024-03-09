using DILDO.server.controllers;
using DILDO.server.models;
using System.Net;
using System.Text;

namespace DILDO.server.Config;
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
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (!model.CancellationToken.IsCancellationRequested)
            {
                byte[] buffer = model.Client.Receive(ref endpoint);
                string encoded = Encoding.UTF32.GetString(buffer);
                handler.InvokePacketReceive(encoded);
            }
        });
    }
}

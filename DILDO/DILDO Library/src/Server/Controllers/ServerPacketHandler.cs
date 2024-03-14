using DILDO.server.models;
using DILDO.server.core.factories;
using DILDO.controllers;

using System.Net;
using System.Net.Sockets;

namespace DILDO.server.controllers
{
    public class ServerPacketHandler : PacketHandler
    {
        private readonly Guid _credentialsPacketID;
        private readonly Guid _credentialsConfirmID;

        private readonly IPAddress? _IPv4;
        private readonly IPAddress? _IPv6;

        private CancellationTokenSource _acceptCts;

        public ServerPacketHandler() : base() 
        {
            _credentialsPacketID = Guid.NewGuid();
            _credentialsConfirmID = Guid.NewGuid();

            IPAddress[] addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var address in addresses)
            {
                if (_IPv6 is null && address.AddressFamily == AddressFamily.InterNetworkV6)
                    _IPv6 = address;

                if (_IPv4 is null && address.AddressFamily == AddressFamily.InterNetwork)
                    _IPv4 = address;

                if (_IPv4 is not null && _IPv6 is not null)
                    break;
            }
        }

        public override void StartPairing()
        {
            ServerState.Instance.Model.Listener.Start();
            Debug.Log<ServerModel>($" <WHI>Servers's <YEL>Listener <DGE>started at port {((IPEndPoint)ServerState.Instance.Model.Listener.LocalEndpoint).Port.ToString()}.");

            _acceptCts = new();
            Task.Run(AcceptConnection);

            base.StartPairing();
        }
        public override void StopPairing()
        {
            _acceptCts?.Cancel();
            ServerState.Instance.Model.Listener.Stop();

            Debug.Log<ServerModel>($" <WHI>Servers's <YEL>Listener <DGE>closed.");
            base.StartPairing();
        }

        protected override void LifeCycle()
        {
            ServerState.Instance.Model.Server.EnableBroadcast = true;
            base.LifeCycle();
        }

        protected override void Pairing()
        {
            var sendingData = PacketFactory.GetFactory
                   (OpCode.BroadcastStringMessage).GetPacket(new string[]
                   {
                       _credentialsPacketID.ToString(),
                       ((int)PacketType.BROADCAST_CREDENTIALS).ToString(),
                       ServerState.Instance.Model.ServerID.ToString(),
                       NetworkingData.This.UserName,
                       _IPv4 is not null? _IPv4.ToString() : "",
                       _IPv6 is not null? _IPv6.ToString() : "",
                       ((IPEndPoint)ServerState.Instance.Model.Listener.LocalEndpoint).Port.ToString(),
                       _credentialsConfirmID.ToString()
                   }).Data;

            if (sendingData is null)
                return;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, ServerModel.DEFAULT_SERVER_SEND_PORT);
            ServerState.Instance.Model.Server.Send(sendingData, sendingData.Length, endpoint);
        }
        private void AcceptConnection()
        {
            while (!_acceptCts.IsCancellationRequested)
            {
                try
                {
                    var client = ServerState.Instance.Model.Listener.AcceptTcpClient();
                    Task.Run(() => ServerState.Instance.ValidateConnection(client));
                }
                catch(Exception ex)
                {
                    Debug.Exception(ex.Message, "no additional explanations.");
                }
            }
        }
        protected override void Communication()
        {

        }
    }
}

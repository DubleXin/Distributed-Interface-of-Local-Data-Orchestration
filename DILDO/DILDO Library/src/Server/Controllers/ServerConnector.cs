using DILDO.server.models;
using DILDO.server.core.factories;
using DILDO.controllers;

using System.Net;
using System.Net.Sockets;
using Client.Controllers;

namespace DILDO.server.controllers
{
    public class ServerConnector : NetworkConnector
    {
        public UdpClient? ReceiveClient { get; private set; }
        public UdpClient? SendClient { get; private set; }

        private readonly Guid _credentialsPacketID;
        private readonly Guid _credentialsConfirmID;

        private readonly IPAddress? _IPv4;
        private readonly IPAddress? _IPv6;

        public ServerConnector() : base() 
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
            Debug.Log<ServerConnector>($" <WHI>Server <DGE>started pairing.");
            base.StartPairing();
        }
        public override void StopPairing()
        {
            Debug.Log<ServerConnector>($" <WHI>Server <DGE>stopped pairing.");
            base.StartPairing();
        }

        protected override void LifeCycle()
        {
            SendClient.EnableBroadcast = true;
            base.LifeCycle();
        }

        protected override void Pairing()
        {
            var sendingData = PacketFactory.GetFactory
                   (OpCode.BroadcastStringMessage).GetPacket(new string[]
                   {
                       _credentialsPacketID.ToString(),
                       ((int)PacketType.BROADCAST_CREDENTIALS).ToString(),
                       ServerState.Instance.Data.ServerID.ToString(),
                       NetworkingData.This.UserName,
                       _IPv4 is not null? _IPv4.ToString() : "",
                       _IPv6 is not null? _IPv6.ToString() : "",
                       ((IPEndPoint)ServerState.Instance.Core.Listener.LocalEndpoint).Port.ToString(),
                       _credentialsConfirmID.ToString()
                   }).Data;

            if (sendingData is null)
                return;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, ServerData.DEFAULT_SERVER_SEND_PORT);
            SendClient.Send(sendingData, sendingData.Length, endpoint);
        }

        public override void Launch()
        {
            ReceiveClient = new(ServerData.DEFAULT_SERVER_RECEIVE_PORT);
            SendClient = new();

            base.Launch();
        }
        public override void Close()
        {
            base.Close();

            SendClient.Close();
            ReceiveClient.Close();
        }
    }
}

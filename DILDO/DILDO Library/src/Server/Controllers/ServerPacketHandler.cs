using DILDO.server.models;
using DILDO.server.core.factories;
using DILDO.controllers;

using System.Net;

namespace DILDO.server.controllers
{
    public class ServerPacketHandler : PacketHandler
    {
        private readonly Guid _credentialsPacketID;
        private readonly Guid _credentialsConfirmID;

        private readonly IPAddress? _IPv4;
        private readonly IPAddress? _IPv6;

        private readonly ushort _port;

        public ServerPacketHandler() : base() 
        {
            _credentialsPacketID = Guid.NewGuid();
            _credentialsConfirmID = Guid.NewGuid();

            IPAddress[] addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var address in addresses)
            {
                if (_IPv6 is null && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    _IPv6 = address;

                if (_IPv4 is null && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    _IPv4 = address;

                if (_IPv4 is not null && _IPv6 is not null)
                    break;
            }
            _port = 47000;
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
                       (_IPv4 is not null? _IPv4.ToString() : ""),
                       (_IPv6 is not null? _IPv6.ToString() : ""),
                       _port.ToString(),
                       _credentialsConfirmID.ToString()
                   }).Data;

            if (sendingData is null)
                return;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, ServerState.Instance.Model.ServerSendPort);
            ServerState.Instance.Model.Server.Send(sendingData, sendingData.Length, endpoint);
        }
        protected override void Communication()
        {

        }
    }
}

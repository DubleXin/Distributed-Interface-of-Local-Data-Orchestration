using DILDO.server.models;
using DILDO.server.core.factories;
using DILDO.controllers;

using System.Net;

namespace DILDO.server.controllers
{
    public class ServerPacketHandler : PacketHandler
    {
        private readonly Guid _credentialsPacketID = Guid.NewGuid();
        private readonly Guid _credentialsConfirmID = Guid.NewGuid();

        public ServerPacketHandler() : base() { }

        protected override void Pairing()
        {
            var sendingData = PacketFactory.GetFactory
                   (OpCode.BroadcastStringMessage).GetPacket(new string[]
                   {
                        _credentialsPacketID.ToString(),
                        ((int)PacketType.BROADCAST_CREDENTIALS).ToString(),
                        ServerState.Instance.Model.ServerID.ToString(),
                        NetworkingData.This.UserName,
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

using DILDO.net.IO;
using DILDO.server.models;
using DILDO.server.core.factories;
using static DILDO.net.IO.PacketReaderBroker;

using System.Collections.Concurrent;
using System.Net;
using DILDO.client.MVM.model;
using ServerModel = DILDO.server.models.ServerModel;

namespace DILDO.server.controllers
{
    public class PacketHandler
    {
        public enum PacketType : int
        {
            BroadcastSession = 0,
            SessionConfirm =1
        }
        public ConcurrentDictionary<Guid, UDPPacket> ReceivedPacketBuffer { get; set; }
        public ConcurrentDictionary<Guid, UDPPacket> SendPacketBuffer { get; set; }
        private ConcurrentDictionary<Guid, int> _packetAttempts;

        public ServerModel @ServerModel { get => _serverModel; }

        private readonly ServerModel _serverModel;
        private readonly Guid _sessionPacketID = Guid.NewGuid();
        private readonly int _sessionOpCode = (int)PacketType.BroadcastSession;
        private readonly Guid _sessionConfirmID = Guid.NewGuid();
        public CancellationTokenSource BroadcastCancellationToken 
        { 
            get; 
            private set; 
        }
        public CancellationTokenSource PacketHandlingCancellationToken
        {
            get;
            private set;
        }
        private string _userName;
        private int _drinkID;

        private readonly ServerState _server;

        public PacketHandler(ServerModel model,
            ServerState server)
        {
            _serverModel = model;
            BroadcastCancellationToken = new();
            PacketHandlingCancellationToken = new();
            ReceivedPacketBuffer = new();
            SendPacketBuffer = new();
            _packetAttempts = new();

            _server = server;
        }

        public void AddPacketToSendQueue(UDPPacket packet, int broadcastAttemts = 1)
        {
            Guid packetGuid = Guid.NewGuid();
            Task.Run(() =>
            {
                while (!_packetAttempts.TryAdd(packetGuid, broadcastAttemts)) { }
                while (!SendPacketBuffer.TryAdd(packetGuid, packet)){}
            });

        }
        public void Broadcast()
        {
            _serverModel.Server.EnableBroadcast = true;
            Task.Run(() =>
            {
                while (!BroadcastCancellationToken.IsCancellationRequested)
                {
                    SendSessionInfo();
                    SendMassagesFromBuffer();
                    TryRemoveSentPackets();
                }
                _server.Dispose();
            });
        }

        private void TryRemoveSentPackets()
        {
            foreach (var item in _packetAttempts)
                if (item.Value <= 0)
                {
                    if (SendPacketBuffer.TryRemove(item.Key, out _))
                        _packetAttempts.TryRemove(item.Key, out _);
                }
        }

        private void SendMassagesFromBuffer()
        {
            foreach (var item in SendPacketBuffer)
            {
                var sendingData = PacketFactory.GetFactory(OpCode.BroadcastStringMessage)
                    .GetPacket(item.Value.RawData).Data;
                if (sendingData is null)
                    continue;
                var endpoint = new IPEndPoint
                    (IPAddress.Broadcast, _serverModel.ServerSendPort);
                if(_packetAttempts.TryGetValue(item.Key, out var attempt) && attempt > 0)
                    _serverModel.Server.
                    Send(sendingData, sendingData.Length, endpoint);
            }
            foreach (var item in _packetAttempts)
                _packetAttempts.TryUpdate(item.Key, item.Value-1, item.Value);
        }

        private void SendSessionInfo()
        {
            var sendingData = PacketFactory.GetFactory
                    (OpCode.BroadcastStringMessage).GetPacket(new string[]
                    {
                        _sessionPacketID.ToString(),
                        _sessionOpCode.ToString(),
                        _serverModel.ServerID.ToString(),
                        _userName,
                        _sessionConfirmID.ToString()
                    }).Data;

            if (sendingData is null)
                return;
            var endpoint = new IPEndPoint
            (IPAddress.Broadcast, _serverModel.ServerSendPort);
            _serverModel.Server.Send(sendingData, sendingData.Length, endpoint);
        }

        public void InvokePacketReceive(string data)
        {
           // Debug.Log("Starting invoking receive method");
            var reader = new PacketReaderBroker(data);
            var packet = new UDPPacket
            {
                Data = reader.GetPacketData(),
                ID = reader.GetID(DataType.PacketID),
                ConfirmID = reader.GetID(DataType.ConfirmID),
                OpCode = reader.GetOpCode(),
                RawData = reader.GetRawPacketData(),

            };
            ReceivedPacketBuffer.TryAdd(packet.ID, packet);
            ReceivedPacketBuffer.TryGetValue(packet.ID, out var packetToInvoke);
            if (packet.Data is not null)
            {
               Debug.Log<PacketHandler>("Invoking the receive action");
               Debug.Log<PacketHandler>("The OnPacketReceived validation -> " +(StateProfile.OnPacketReceived is null));
                _server.RaisePacketReceiver(packetToInvoke, "public void InvokePacketReceive(string data)");
            }
        }

    }
}

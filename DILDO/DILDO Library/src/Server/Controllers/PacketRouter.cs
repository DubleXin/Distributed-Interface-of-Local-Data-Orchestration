using DILDO.net.IO;
using DILDO.server.models;
using DILDO.server.core.factories;
using static DILDO.net.IO.PacketReaderBroker;
using ServerModel = DILDO.server.models.ServerModel;

using System.Collections.Concurrent;
using System.Net;

namespace DILDO.server.controllers
{
    public class PacketRouter
    {
        public int TickRate { get => _tickDelay * 1024; set => _tickDelay = 1024 / value; }
        private int _tickDelay;

        public enum PacketType : int
        {
            BroadcastCredentials = 0,
            SessionConfirm =1
        }
        public ConcurrentDictionary<Guid, UDPPacket> ReceivedPacketBuffer { get; set; }
        public ConcurrentDictionary<Guid, UDPPacket> SendPacketBuffer { get; set; }

        private readonly ConcurrentDictionary<Guid, int> _packetAttempts;

        private readonly ServerState _server;

        private readonly ServerModel _serverModel;
        private readonly Guid _credentialsPacketID = Guid.NewGuid();
        private readonly int  _credentialsOpCode = (int)PacketType.BroadcastCredentials;
        private readonly Guid _credentialsConfirmID = Guid.NewGuid();

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

        public bool BroadcastCredentials;

        public PacketRouter(ServerModel model,
                            ServerState server)
        {
            _serverModel = model;
            BroadcastCancellationToken = new();
            PacketHandlingCancellationToken = new();
            ReceivedPacketBuffer = new();
            SendPacketBuffer = new();
            _packetAttempts = new();

            _server = server;

            BroadcastCredentials = false;
            TickRate = 32;
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
                    if(BroadcastCredentials)
                        SendCredentials();

                    SendMassagesFromBuffer();
                    TryRemoveSentPackets();
                    Thread.Sleep(_tickDelay);
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
        private void SendCredentials()
        {
            var sendingData = PacketFactory.GetFactory
                    (OpCode.BroadcastStringMessage).GetPacket(new string[]
                    {
                        _credentialsPacketID.ToString(),
                        _credentialsOpCode.ToString(),
                        _serverModel.ServerID.ToString(),
                        NetworkingData.This.UserName,
                        _credentialsConfirmID.ToString()
                    }).Data;

            if (sendingData is null)
                return;
            var endpoint = new IPEndPoint
            (IPAddress.Broadcast, _serverModel.ServerSendPort);
            _serverModel.Server.Send(sendingData, sendingData.Length, endpoint);
        }

        public void InvokePacketReceive(string data)
        {
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
               Debug.Log<PacketRouter>("<CYA>The OnPacketReceived validation -> " +(StateProfile.OnPacketReceived is null));
                _server.RaisePacketReceiver(packetToInvoke, "public void InvokePacketReceive(string data)");
            }
        }

    }
}

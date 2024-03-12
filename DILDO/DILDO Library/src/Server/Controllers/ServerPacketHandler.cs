using DILDO.net.IO;
using DILDO.server.models;
using DILDO.server.core.factories;
using static DILDO.net.IO.PacketReaderBroker;
using ServerModel = DILDO.server.models.ServerModel;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using DILDO.client.MVM.model;

namespace DILDO.server.controllers
{
    public abstract class PacketHandler : IDisposable
    {
        public enum PacketType : int
        {
            BROADCAST_CREDENTIALS = 0,
            SessionConfirm = 1
        }

        #region FIELDS
        protected ConcurrentDictionary<User, NetworkStream> _streams;
        #endregion

        #region CONFIG
        private int _tickRate
        {
            get => _tickDelay * 1024;
            set => _tickDelay = 1024 / value;
        }
        private int _tickDelay;

        #region UDP ADDON
        public ConcurrentDictionary<Guid, UDPPacket> ReceivedPacketBuffer { get; set; }
        public ConcurrentDictionary<Guid, UDPPacket> SendPacketBuffer { get; set; }

        private readonly ConcurrentDictionary<Guid, int> _packetAttempts;
        #endregion

        #endregion

        #region CANCELLATION TOKEN SOURCES

        public CancellationTokenSource LifeCycleCTS { get; protected set; }
        public CancellationTokenSource PairingCTS { get; protected set; }
        public CancellationTokenSource CommunicationCTS { get; protected set; }

        #endregion

        #region CONSTRUCTOR

        public PacketHandler(bool immediatePairing = false)
        {
            _streams = new();

            ReceivedPacketBuffer = new();
            SendPacketBuffer = new();
            _packetAttempts = new();

            LifeCycleCTS = new();
            PairingCTS = new();
            CommunicationCTS = new();

            SetStandardConfig();
        }

        #endregion

        #region CODE INTERFACE

        #region PUBLIC CODE INTERFACE
        public void Launch() => Task.Run(LifeCycle);

        public virtual void StartPairing() => Task.Run(() => 
        { 
            while (PairingCTS.IsCancellationRequested)
                PairingCTS.TryReset();
        });
        public virtual void StopPairing()
            => PairingCTS.Cancel();

        public virtual void StartCommunication() => Task.Run(() =>
        {
            while (CommunicationCTS.IsCancellationRequested)
                CommunicationCTS.TryReset();
        });
        public virtual void StopCommunication()
            => CommunicationCTS.Cancel();

        public void Dispose()
        {
            LifeCycleCTS.Dispose();
            CommunicationCTS.Dispose();
            PairingCTS.Dispose();
        }

        #endregion

        #region PRIVATE/PROTECTED CODE INTERFACE

        private void LifeCycle()
        {
            while (!LifeCycleCTS.IsCancellationRequested)
            {
                if (!PairingCTS.IsCancellationRequested)
                    Pairing();
                if (!CommunicationCTS.IsCancellationRequested)
                    Communication();

                Thread.Sleep(_tickDelay);
            }
            Dispose();
        }

        protected abstract void Pairing();
        protected abstract void Communication();

        private void SetStandardConfig() => SetConfig();
        public virtual void SetConfig(int tickRate = 64)
        {
            _tickRate = tickRate;
        }

        #endregion

        #endregion
    }

    public class ServerPacketHandler : PacketHandler
    {
        private readonly Guid _credentialsPacketID = Guid.NewGuid();
        private readonly Guid _credentialsConfirmID = Guid.NewGuid();

        public bool BroadcastCredentials
        { set => (value ? (Action)StartPairing : StopPairing).Invoke(); }

        public ServerPacketHandler() : base() { }

        public override void StartPairing()
        {
            ServerState.Instance.Model.Server.EnableBroadcast = true;
            base.StartPairing();
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

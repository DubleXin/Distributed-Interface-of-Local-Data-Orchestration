using DILDO.server.models.connectors;

using System.Collections.Concurrent;
using System.Net.Sockets;
using DILDO.client.models;

namespace DILDO.controllers
{
    public abstract class NetworkConnector
    {
        protected Task? LifeCycleTask;

        public enum PacketType : int
        {
            BROADCAST_CREDENTIALS = 0
        }

        #region FIELDS
        protected ConcurrentDictionary<UserData, NetworkStream>? _streams;
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

        #region CANCELLATION

        public CancellationTokenSource? LifeCycleCTS { get; protected set; }
        public bool IsPairing { get; private set; }
        public bool IsCommunicating { get; private set; }

        #endregion

        #region CONSTRUCTOR

        public NetworkConnector(bool immediatePairing = false)
        {
            _streams = new();

            ReceivedPacketBuffer = new();
            SendPacketBuffer = new();
            _packetAttempts = new();

            IsPairing = immediatePairing;
            IsCommunicating = false;

            SetStandardConfig();
        }

        #endregion

        #region CODE INTERFACE

        #region PUBLIC CODE INTERFACE
        public virtual void Launch()
        {
            LifeCycleCTS = new();
            LifeCycleTask = Task.Run(LifeCycle);
        }
        public virtual void StartPairing() => IsPairing = true;
        public virtual void StopPairing() => IsPairing = false;

        public virtual void Close()
        {
            LifeCycleCTS.Cancel();
            LifeCycleTask.Wait();
            LifeCycleCTS.Dispose();
        }
        #endregion

        #region PRIVATE/PROTECTED CODE INTERFACE

        protected virtual void LifeCycle()
        {
            while (!LifeCycleCTS.IsCancellationRequested)
            {
                if (IsPairing)
                    Pairing();

                Thread.Sleep(_tickDelay);
            }
        }

        protected abstract void Pairing();

        private void SetStandardConfig() => SetConfig();
        public virtual void SetConfig(int tickRate = 1)
        {
            _tickRate = tickRate;
        }

        #endregion

        #endregion
    }
}

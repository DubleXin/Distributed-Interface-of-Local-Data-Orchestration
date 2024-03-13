using DILDO.server.models;

using System.Collections.Concurrent;
using System.Net.Sockets;
using DILDO.client.models;

namespace DILDO.controllers
{
    public abstract class PacketHandler : IDisposable
    {
        public Action? OnDisposed;

        public enum PacketType : int
        {
            BROADCAST_CREDENTIALS = 0,
            SessionConfirm = 1
        }

        #region FIELDS
        protected ConcurrentDictionary<UserData, NetworkStream> _streams;
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

        public CancellationTokenSource LifeCycleCTS { get; protected set; }
        public bool IsPairing { get; private set; }
        public bool IsCommunicating { get; private set; }

        #endregion

        #region CONSTRUCTOR

        public PacketHandler(bool immediatePairing = false)
        {
            _streams = new();

            ReceivedPacketBuffer = new();
            SendPacketBuffer = new();
            _packetAttempts = new();

            LifeCycleCTS = new();

            IsPairing = immediatePairing;
            IsCommunicating = false;

            SetStandardConfig();
        }

        #endregion

        #region CODE INTERFACE

        #region PUBLIC CODE INTERFACE
        public void Launch() => Task.Run(LifeCycle);

        public virtual void StartPairing() => IsPairing = true;
        public virtual void StopPairing() => IsPairing = false;

        public  virtual void StartCommunication() => IsCommunicating = true;
        public virtual void StopCommunication() => IsCommunicating = false;

        public void Dispose()
        {
            LifeCycleCTS.Dispose();
            OnDisposed?.Invoke();
        }
        #endregion

        #region PRIVATE/PROTECTED CODE INTERFACE

        protected virtual void LifeCycle()
        {
            while (!LifeCycleCTS.IsCancellationRequested)
            {
                if (IsPairing)
                    Pairing();
                if (IsCommunicating)
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
}

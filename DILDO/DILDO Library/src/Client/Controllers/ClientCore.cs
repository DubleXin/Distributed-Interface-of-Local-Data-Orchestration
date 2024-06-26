﻿using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using static StreamUtil;

namespace DILDO.client;

public class ClientCore
{
    private Task? _current;
    private bool _active;

    public Guid ConnectedServer { get; private set; }
    public TcpClient? TcpClient { get; private set; }

    public void Launch()
    {
        _active = true;
        ConnectedServer = Guid.Empty;
    }
    public void Close()
    {
        _active = false;
        _current.Wait();
        ConnectedServer = Guid.Empty;
    }

    private void LifeCycle()
    {
        NetworkStream stream = TcpClient.GetStream();
        while (_active)
        {
            try
            {
                var packet = Read(stream);
                if(packet is not null 
                    && packet.ObjectData is not null
                    && !string.IsNullOrEmpty(packet.TypeName))
                    NetworkingOutput.EnqueuePacket(packet);
            }
            catch (Exception ex)
            {
                if (_active)
                {
                    Debug.Exception(ex.Message, "no additional info. ");
                    Reconnect();
                }
            }
            Thread.Sleep(100);
        }
    }

    public void Connect(Guid guid)
    {
        Debug.Log<ClientState>($"<DMA> Requesting connection to server named: <CYA>" +
            $"{ClientState.Instance.Data.Servers[guid].ServerName}");

        if (!ClientState.Instance.Data.Servers.TryGetValue(guid, out var serverData))
            return;

        try
        {
            TcpClient = new TcpClient();

            if (serverData.V4 is not null)
                TcpClient.Connect(serverData.V4.Address, serverData.V4.Port);
            else if (serverData.V6 is not null)
                TcpClient.Connect(serverData.V6.Address, serverData.V6.Port);

            NetworkStream stream = TcpClient.GetStream();
            _current = Task.Run(LifeCycle);

            ConnectedServer = guid;
        }
        catch (Exception ex) { Debug.Exception(ex.Message, "no additional info."); }
    }
    private void Reconnect()
    {
        TcpClient.Close();
        Connect(ConnectedServer);
    }
    public void Disconnect()
    {
        TcpClient.Close();
        Close();
        _active = true;

        Debug.Log<ClientCore>(" <DGE>Successfully disconnected from server ");
    }

    public void Send(object obj)
    {
        if (obj == null) 
            return;
        try
        {
            Write(TcpClient.GetStream(), obj);
        }
        catch (Exception e)
        {
            Debug.Log<ClientCore>(e.Message);
            Reconnect();
        }
    }

}
using System.Net.Sockets;
using System.Text;

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
        while (_active && TcpClient.Connected)
        {
            try
            {
                var message = Encoding.UTF8.GetString(StreamUtil.Read(stream));
                Debug.Log<ClientState>($" <DYE>{message}");
            }
            catch (Exception ex)
            {
                Debug.Exception(ex.Message, "no additional info.");
                Reconnect();
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

        Debug.Log<ClientState>($"<YEL>  " +
            $"{(serverData.V4 is null ? "" : $"IPv4 : {serverData.V4}")} , " +
            $"{(serverData.V6 is null ? "" : $"IPv6 : {serverData.V6}")}");

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
        ConnectedServer = Guid.Empty;
    }

    public void Send(string pInput)
    {
        if (pInput == null || pInput.Length == 0) return;

        try
        {
            byte[] outBytes = Encoding.UTF8.GetBytes(pInput);
            StreamUtil.Write(TcpClient.GetStream(), outBytes);
        }
        catch (Exception e)
        {
            Debug.Log<ClientCore>(e.Message);
            Reconnect();
        }
    }

}
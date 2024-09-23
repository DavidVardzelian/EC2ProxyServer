using System.Net;
using System.Net.Sockets;

class ProxyServer
{
    private const int Port = 6666;

    public static async Task Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Proxy server started on port {Port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => ClientHandler.HandleClient(client));
        }
    }
}
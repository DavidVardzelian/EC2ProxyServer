using System.Net;
using System.Net.Sockets;
using System.Text;

class ProxyServer
{
    private const int BufferSize = 8192;
    private const int Port = 6666;
    private const int TimeoutMiliseconds = 5000;

    public static async Task Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Proxy server started on port {Port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            NetworkStream clientStream = client.GetStream();

            clientStream.ReadTimeout = TimeoutMiliseconds; 
            clientStream.WriteTimeout = TimeoutMiliseconds; 

            byte[] buffer = new byte[BufferSize];
            int bytesRead = 0;

            try
            {
                 bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Client read tiemout or error: {ex.Message}");
                return; 
            }
            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received request:\n{request}");

            string[] requestLines = request.Split(new[] { "\r\n" }, StringSplitOptions.None);
            string[] requestLine = requestLines[0].Split(' ');

            if (requestLine[0] == "CONNECT")
            {
                await HandleConnectRequest(clientStream, requestLine[1]);
            }
            else
            {
                await HandleHttpRequest(clientStream, buffer, bytesRead, requestLines);
            }
        }
    }

    private static async Task HandleConnectRequest(NetworkStream clientStream, string hostPort)
    {
        string[] hostPortSplit = hostPort.Split(':');
        string host = hostPortSplit[0];
        int port = hostPortSplit.Length == 2 ? int.Parse(hostPortSplit[1]) : 443;

        TcpClient server = null;
        try
        {
            server = new TcpClient();
            await server.ConnectAsync(host, port);

            NetworkStream serverStream = server.GetStream();
            serverStream.ReadTimeout = TimeoutMiliseconds;
            serverStream.WriteTimeout = TimeoutMiliseconds;

            byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
            await clientStream.WriteAsync(response, 0, response.Length);

            Task clientToServer = clientStream.CopyToAsync(serverStream);
            Task serverToClient = serverStream.CopyToAsync(clientStream);

            await Task.WhenAny(clientToServer, serverToClient);

        }
        catch (IOException ex)
        {
            Console.WriteLine($"Server read/write timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
        finally
        {
            if (server != null)
            {
                server.Close(); 
            }
        }
    }

    private static async Task HandleHttpRequest(NetworkStream clientStream, byte[] buffer, int bytesRead, string[] requestLines)
    {
        string host = GetHost(requestLines);
        if (string.IsNullOrEmpty(host))
        {
            Console.WriteLine("Host not found in the request");
            return;
        }
        TcpClient server = null;
        try
        {
            server = new TcpClient();
            await server.ConnectAsync(host, 80);

            NetworkStream serverStream = server.GetStream();
            serverStream.ReadTimeout=TimeoutMiliseconds;
            serverStream.WriteTimeout = TimeoutMiliseconds;

            await serverStream.WriteAsync(buffer, 0, bytesRead);
            try
            {
                bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Server read timeout: {ex.Message}");
                return; 
            }

            await clientStream.WriteAsync(buffer, 0, bytesRead);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Server read/write timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
        finally
        {
            if (server != null)
            {
                server.Close(); // Ensure the server TcpClient is closed
            }
        }
    }

    private static string GetHost(string[] requestLines)
    {
        foreach (string line in requestLines)
        {
            if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(6).Trim();
            }
        }
        return null;
    }
}

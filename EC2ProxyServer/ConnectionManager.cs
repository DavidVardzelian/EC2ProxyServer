using System.Text;
using System.Net.Sockets;

class ConnectionManager
{
    private const int TimeoutMilliseconds = 5000;

    public static async Task HandleConnectRequest(NetworkStream clientStream, string hostPort)
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
            serverStream.ReadTimeout = TimeoutMilliseconds;
            serverStream.WriteTimeout = TimeoutMilliseconds;

            byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
            await clientStream.WriteAsync(response, 0, response.Length);

            Task clientToServer = clientStream.CopyToAsync(serverStream);
            Task serverToClient = serverStream.CopyToAsync(clientStream);

            await Task.WhenAny(clientToServer, serverToClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
        finally
        {
            server?.Close();
        }
    }

    public static async Task HandleHttpRequest(NetworkStream clientStream, byte[] buffer, int bytesRead, string[] requestLines)
    {
        string host = Utilities.GetHost(requestLines);
        if (string.IsNullOrEmpty(host))
        {
            Logger.Log("Host not found in the request");
            return;
        }

        string requestLine = requestLines[0];
        string cacheKey = $"{host}-{requestLine}";

        byte[] cachedResponse = Cache.GetFromCache(cacheKey);
        if (cachedResponse != null)
        {
            Logger.Log($"Serving cached response for {cacheKey}");
            await clientStream.WriteAsync(cachedResponse, 0, cachedResponse.Length);
            return;
        }

        TcpClient server = null;
        try
        {
            server = new TcpClient();
            await server.ConnectAsync(host, 80);

            NetworkStream serverStream = server.GetStream();
            serverStream.ReadTimeout = TimeoutMilliseconds;
            serverStream.WriteTimeout = TimeoutMilliseconds;

            await serverStream.WriteAsync(buffer, 0, bytesRead);
            bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length);

            Cache.AddToCache(cacheKey, buffer.Take(bytesRead).ToArray());

            await clientStream.WriteAsync(buffer, 0, bytesRead);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error connecting to server", ex);
        }
        finally
        {
            server?.Close();
        }
    }
}
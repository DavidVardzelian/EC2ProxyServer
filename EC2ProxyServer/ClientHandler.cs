using System.Text;
using System.Net.Sockets;

class ClientHandler
{
    private const int TimeoutMilliseconds = 5000;

    public static async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            NetworkStream clientStream = client.GetStream();
            clientStream.ReadTimeout = TimeoutMilliseconds;
            clientStream.WriteTimeout = TimeoutMilliseconds;

            byte[] buffer = new byte[8192];
            int bytesRead = 0;

            try
            {
                bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                Logger.LogError("Client read timeout or error", ex);
                return;
            }

            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Logger.Log($"Received request:\n{request}");

            await RequestProcessor.ProcessRequest(clientStream, buffer, bytesRead, request);
        }
    }
}
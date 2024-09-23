using System.Net.Sockets;
using System.Text;

class RequestProcessor
{
    public static async Task ProcessRequest(NetworkStream clientStream, byte[] buffer, int bytesRead, string request)
    {
        string[] requestLines = request.Split(new[] { "\r\n" }, StringSplitOptions.None);
        string[] requestLine = requestLines[0].Split(' ');

        if (requestLine[0] == "CONNECT")
        {
            await ConnectionManager.HandleConnectRequest(clientStream, requestLine[1]);
        }
        else
        {
            await ConnectionManager.HandleHttpRequest(clientStream, buffer, bytesRead, requestLines);
        }
    }
}
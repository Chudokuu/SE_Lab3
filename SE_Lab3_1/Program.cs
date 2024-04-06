using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    static async Task Main(string[] args)
    {
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipPoint);
        socket.Listen();
        Console.WriteLine("Server is running\n");

        while (true)
        {
            await WaitForConnection(socket);
        }
    }

    static async Task WaitForConnection(Socket socket)
    {
        Console.WriteLine("Waiting for connect...");
        using Socket client = await socket.AcceptAsync();
        Console.WriteLine($"Client connected. Client address: {client.RemoteEndPoint}");

        byte[] buffer = new byte[512];
        int bytesReceived = await client.ReceiveAsync(buffer, SocketFlags.None);
        string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
        Console.WriteLine($"Data received from client: {data}");

        string response = "";

        if (data == "exit")
        {
            Console.WriteLine("Exit command received. Server is shutting down.");
            Environment.Exit(0);
        }

        string[] request = data.Split(' ');
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (request[0] == "PUT"  || request[0] == "GET" ||  request[0] == "DELETE")
        {
            string filepath = Path.Combine(desktopPath, "data", request[1]);

            if (request[0] == "PUT")
            {
                DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(desktopPath, "data"));
                FileInfo file = new FileInfo(filepath);
                if (file.Exists)
                {
                    response += "403";
                }
                else
                {
                    StreamWriter sw = file.CreateText();
                    for (int i = 2; i < request.Length; i++)
                    {
                        sw.Write(request[i] + " ");
                    }
                    sw.Close();
                    response += "200";
                }
            }
            else if (request[0] == "GET")
            {
                FileInfo file = new FileInfo(filepath);
                if (file.Exists)
                {
                    response += "200 " + File.ReadAllText(filepath);
                }
                else
                {
                    response += "404";
                }
            }
            else if (request[0] == "DELETE")
            {
                FileInfo file = new FileInfo(filepath);
                if (file.Exists)
                {
                    file.Delete();
                    response += "200";
                }
                else
                {
                    response += "404";
                }
            }
        }
        else
        {
            response = "Invalid command";
        }

        // Отправляем результат клиенту
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await client.SendAsync(responseBytes, SocketFlags.None);
        Console.WriteLine($"Result sent to client: {response}\n");
    }
}
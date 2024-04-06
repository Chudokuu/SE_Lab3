using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Client
{
    static async Task Main(string[] args)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync("127.0.0.1", 8888);
            Console.WriteLine($"Connection to {socket.RemoteEndPoint} established");
            string userInput = "";
            int action;
            do
            {

                Console.Write("Enter action (1 - get a file, 2 - create a file, 3 - delete a file, exit to quit): > ");
                userInput = Console.ReadLine().ToLower();

                if (userInput == "exit")
                {
                    // Отправляем команду exit и завершаем цикл
                    byte[] exitBytes = Encoding.UTF8.GetBytes("exit");
                    await socket.SendAsync(exitBytes, SocketFlags.None);
                    Console.WriteLine("Exit command sent.");
                    return;
                }

                if (!int.TryParse(userInput, out action) ||  action < 1 || action > 3)
                {
                    Console.WriteLine("Invalid action. Please enter 1, 2, 3, or exit.");

                }
            } while (!int.TryParse(userInput, out action) || action < 1 || action > 3);


            Console.Write("Enter filename: > ");
            string filename = Console.ReadLine();

            string content = "";
            if (action == 2)
            {
                Console.Write("Enter file content: > ");
                content = Console.ReadLine();
            }

            string request = "";
            if (action == 1)
            {
                request += "GET " + filename;
            }
            else if (action == 2)
            {
                request += "PUT " + filename + " " + content;
            }
            else if (action == 3)
            {
                request += "DELETE " + filename;
            }

            var requestBytes = Encoding.UTF8.GetBytes(request);
            await socket.SendAsync(requestBytes, SocketFlags.None);
            Console.WriteLine("The request was sent.");

            byte[] buffer = new byte[512];
            int bytesReceived = await socket.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

            if (action == 2)
            {
                if (response == "403")
                {
                    Console.WriteLine("The response says that file already exists!");
                }
                else
                {
                    Console.WriteLine("The response says that file was created!");
                }
            }
            else if (action == 1)
            {
                if (response.StartsWith("200"))
                {
                    string[] parts = response.Split(' ');
                    Console.WriteLine($"The content of the file is: {parts[1]}");
                }
                else if (response == "404")
                {
                    Console.WriteLine("The response says that the file was not found!");
                }
            }
            else if (action == 3)
            {
                if (response == "200")
                {
                    Console.WriteLine("The response says that the file was successfully deleted!");
                }
                else if (response == "404")
                {
                    Console.WriteLine("The response says that the file was not found!");
                }
            }

        }
        catch (SocketException)
        {
            Console.WriteLine($"Failed to connect to {socket.RemoteEndPoint}");
        }
    }
}
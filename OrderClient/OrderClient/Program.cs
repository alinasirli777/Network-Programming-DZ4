using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

int port = 5000;

using TcpClient client = new();

IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, port);

await client.ConnectAsync(iPEndPoint);

using var stream = client.GetStream();

Console.WriteLine("===== OrderClient =====");

while (true)
{
    Console.Write("Введите заказ или EXIT: ");
    string input = Console.ReadLine()?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(input)) continue;
    await stream.WriteAsync(Encoding.UTF8.GetBytes(input));
    if (input == "EXIT") break;

    byte[] buffer = new byte[1024];
    var bytesRead = await stream.ReadAsync(buffer);
    if (bytesRead > 0)
    {
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Ответ сервера: {receivedMessage}");
    }
}


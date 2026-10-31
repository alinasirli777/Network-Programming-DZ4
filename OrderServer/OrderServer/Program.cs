using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

int port = 5000;
int totalOrdersCount = 0;
int totalOrdersSum = 0;

using TcpListener listener = new(IPAddress.Loopback, port);
listener.Start();

Console.WriteLine("Сервер запущен. Ожидание подключений на порту 5000...");

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    Console.WriteLine($"Подключился новый клиент: {client.Client.RemoteEndPoint}.");
    _ = ProcessClientAsync(client);
}

async Task ProcessClientAsync(TcpClient client)
{
    int clientTotal = 0;
    using var stream = client.GetStream();
    byte[] buffer = new byte[1024];

    while (true)
    {
        var bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead == 0) break;

        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

        Console.WriteLine($"Получено сообщение: {message}");

        if (message == "EXIT") break;

        string[] words = message.Split(";");

        int wordsCount = words.Length;

        if (words[0] != "ORDER")
        {
            string errorMessage = "Ошибка: Получена неизвестная команда от клиента.";
            Console.WriteLine($"Лог сервера: {errorMessage}");
            await stream.WriteAsync(Encoding.UTF8.GetBytes(errorMessage));
            continue;
        }
        else if (wordsCount != 4)
        {
            string errorMessage = "Ошибка: Неверный формат заказа.";
            Console.WriteLine($"Лог сервера: {errorMessage}");
            await stream.WriteAsync(Encoding.UTF8.GetBytes(errorMessage));
            continue;
        }
        else if (string.IsNullOrWhiteSpace(words[1]))
        {
            string errorMessage = "Ошибка: название товара не может быть пустым\n";
            Console.WriteLine($"Лог сервера: {errorMessage}");
            await stream.WriteAsync(Encoding.UTF8.GetBytes(errorMessage));
            continue;
        }
        else if (!int.TryParse(words[2], out int amount) || !int.TryParse(words[3], out int price))
        {
            string errorMessage = "Ошибка: количество и цена должны быть числами\n";
            Console.WriteLine($"Лог сервера: {errorMessage}");
            await stream.WriteAsync(Encoding.UTF8.GetBytes(errorMessage));
            continue;
        }
        else if (amount <= 0 || price < 0)
        {
            string errorMessage = "Ошибка: количество должно быть положительным\n";
            Console.WriteLine($"Лог сервера: {errorMessage}");
            await stream.WriteAsync(Encoding.UTF8.GetBytes(errorMessage));
            continue;
        }
        else
        {
            int total = amount * price;
            totalOrdersCount++;
            totalOrdersSum += total;
            clientTotal += total;
            Console.WriteLine($"[Статистика сервера] Всего заказов: {totalOrdersCount}, Общая сумма: {totalOrdersSum}");
            string response = $"Заказ принят: {words[1]}, Стоимость: {total}\n(Всего за сессию: {clientTotal})\n";
            await stream.WriteAsync(Encoding.UTF8.GetBytes(response));
        }
    }

    Console.WriteLine($"Клиент завершил соединение. Итог покупок за сессию: {clientTotal}");
}
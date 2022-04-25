using System;
using System.Net;

namespace ChatProject_v2
{
    class Program
    {
        private const int OutputMessagesNumber = 20;

        static void Main(string[] args)
        {
            Console.Write("Введите имя пользователя: ");
            string name = Console.ReadLine();
            Console.Write("Введите IP: ");
            IPAddress ip = IPAddress.Parse(Console.ReadLine());

            User user = new User(name, ip);
            user.Connect();

            Console.Clear();
            string message = "";
            while (true)
            {
                message = Console.ReadLine();

                if (message.CompareTo("--exit") == 0)
                {
                    user.Disconnect();
                    break;
                }

                if (message.CompareTo("--log") == 0)
                {
                    user.PrintChatLog();
                    user.PrintLastMessages(OutputMessagesNumber);
                    continue;
                }

                user.SendMessage(message);
                user.PrintLastMessages(OutputMessagesNumber);
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}

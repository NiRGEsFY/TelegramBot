using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;
using System.Web;

namespace TelegramBot
{
    internal class LogCreator
    {
        public static string fileName = $"Log[{DateTime.Today.ToString().Replace(':','.')}].txt";

        public static string wayToLibrary = "E:\\LogTelegramBot\\TextLog\\";
        
        public string wayToTextLog = wayToLibrary + fileName;

        public void LogUser(Message msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string contentLog = $"[{msg.Date}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()}){msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):{msg.Text}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{msg.Date}]");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"(Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()})");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg.Text);
            StreamWriter streamWriter = new StreamWriter(wayToTextLog, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public void LogBot(Message msg, string msgBot)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string contentLog = $"[{DateTime.Now}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()})Bot:\"{msgBot}\" to {msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id})";
            Console.WriteLine(contentLog);
            StreamWriter streamWriter = new StreamWriter(wayToTextLog, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public void Action(string work, string worker, bool endWork)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            if (endWork == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            string contentLog = $"[{DateTime.Now}]Bot:\"{worker}\"({work}) is {endWork}";
            Console.WriteLine(contentLog);
            StreamWriter streamWriter = new StreamWriter(wayToTextLog, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
    }
}

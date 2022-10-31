using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;
using System.Web;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.Bot;
using Microsoft.VisualBasic;

namespace TelegramBot
{
    internal class LogCreator
    {
        public string CreatorWayToLog()
        {
            string fileName = $"Log[{DateTime.Today.ToString().Replace(':', '.')}].txt";
            string wayToFolder = "E:\\LogTelegramBot\\TextLog\\";
            return wayToFolder + fileName;
        }
        private async void CreatorFile(Message msg, ITelegramBotClient botClient, string fileId, string type)
        {
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;
            Dictionary<string, int> fileType = new Dictionary<string, int> { 
                { "Audio", 0 },
                { "Video", 1},
                { "Photo", 2}
            };
            if (!fileType.ContainsKey(type))
            {
                Action("take same file from channel", "LogCreator", false);
            }
            string[] underTypeOfFile = new string[] 
            { 
                "mp3",
                "mp4",
                "png" 
            };
            string fileName = $@"{DateTime.Now.ToString().Replace(":", ".")}_{fileInfo.FileUniqueId}.{underTypeOfFile[fileType[type]]}";
            string wayToFolder = @$"E:\LogTelegramBot\FileLibrary\{type}\";
            await using FileStream fileStream = System.IO.File.OpenWrite(wayToFolder + fileName);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();
        }

        public void LogUser(Message msg)
        {
            string fileWay = CreatorWayToLog();
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
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public void LogBot(Message msg, string msgBot)
        {
            string fileWay = CreatorWayToLog();
            Console.ForegroundColor = ConsoleColor.Green;
            string contentLog = $"[{DateTime.Now}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()})Bot:\"{msgBot}\" to {msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id})";
            Console.WriteLine(contentLog);
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public void Action(string work, string worker, bool endWork)
        {
            string fileWay = CreatorWayToLog();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (endWork == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            string contentLog = $"[{DateTime.Now}]Bot:\"{worker}\"({work}) is {endWork}";
            Console.WriteLine(contentLog);
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public async void Photo(Message msg, ITelegramBotClient botClient)
        {
            string fileWay = CreatorWayToLog();
            string contentLog = $"[{msg.Date}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()}){msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):Photo";
            OutInConsole(msg);
            string fileId = msg.Photo.Last().FileId;
            CreatorFile(msg,botClient,fileId,"Photo");
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public async void Video(Message msg, ITelegramBotClient botClient)
        {
            string fileWay = CreatorWayToLog();
            string contentLog = $"[{msg.Date}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()}){msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):Video";
            OutInConsole(msg);
            var fileId = msg.Video.FileId;
            CreatorFile(msg, botClient, fileId, "Video");
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }
        public async void Audio(Message msg, ITelegramBotClient botClient)
        {
            string fileWay = CreatorWayToLog();
            string contentLog = $"[{msg.Date}](Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()}){msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):Audio";
            OutInConsole(msg);
            var fileId = msg.Audio.FileId;
            CreatorFile(msg, botClient, fileId, "Audio");
            StreamWriter streamWriter = new StreamWriter(fileWay, true, Encoding.Default);
            streamWriter.WriteLine(contentLog);
            streamWriter.Close();
        }

        private void OutInConsole(Message msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(msg.Date);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"(Channel:{msg.Chat.Title ?? msg.Chat.Type.ToString()})");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{msg.From.FirstName ?? msg.From.Username ?? msg.From.LastName ?? "NullName"}({msg.From.Id}):");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg.Text ?? "NullText");
        }
    }
}

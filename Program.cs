using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal class Program
    {
        private static readonly LogCreator _log = new ();
        public static readonly WorkerWithUserInDB _dataBase = new ();
        private static readonly CommandsBot _commandsBot = new ();
        private static ITelegramBotClient _botClient;
        private static Message _msg;

        static void Main(string[] args)
        {
            TelegramBotClient client = new ("5689722010:AAGIwvLsPyXG2heUxo_pHxAz6PQN9M1togY");
            client.StartReceiving(Update, Error);
            _dataBase.StartWorkWithDB();
            while (true)
            {
                string textFromBot = Console.ReadLine();
                BotMesseng(textFromBot);
            }
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var msg = update?.Message;
            _msg = msg;
            _botClient = botClient;
            try
            {
                if (msg != null && msg.Text != null)
                {
                    if (msg.Text[0] == '!')
                    {
                        _commandsBot.ExecuterComand(msg, botClient, _dataBase);
                    }
                    _log.LogUser(msg);
                    if (msg.Chat.Type.ToString() != "Private")
                    {
                        _dataBase.WorkWithGroupLevel(msg);
                    }
                    else
                    {
                        _dataBase.WorkWithLevel(msg);
                    }
                }
                else if (msg != null && msg.Photo != null)
                {
                    _log.Photo(msg, botClient);
                }
                else if (msg != null && msg.Video != null)
                {
                    _log.Video(msg, botClient);
                }
                else if (msg != null && msg.Audio != null)
                {
                    _log.Audio(msg, botClient);
                }
                else if (msg != null && msg.Voice != null)
                {
                    _log.Voice(msg, botClient);
                    string msgText = "";
                    for (int i = 0; i < msg.Voice.Duration; i++)
                    {
                        msgText += Convert.ToString(i);
                    }
                    msg.Text = msgText;
                    _dataBase.WorkWithLevel(msg);
                }
                else
                {
                    return;
                }
            }
            catch
            {
                _log.Action("Working","Bot",false);
            }
        }
        async static Task BotMesseng(string messenge)
        {
            try
            {
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, messenge);
                _log.LogBot(_msg, messenge);
            }
            catch
            {
                _log.Action("send messeng to near chanel", "Program", false);
            }
        }


        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            return Task.CompletedTask;
        }
    }
}
using System.Data.SqlClient;
using Telegram.Bot;
using System.Timers;
using Telegram.Bot.Types;
using System.Threading;

namespace TelegramBot
{
    internal class CommandsBot
    {
        private string _chatId = "";
        private  LogCreator _log = new();
        private SqlCommand _command;
        private SqlConnection _sqlConnection;
        private ITelegramBotClient _botClient;
        private Dictionary<long, Battle> _battleFields = new();
        private Dictionary<string, long> _battleWaiters = new();
        private Dictionary<long, int> _battleFieldsTimeForDel = new();
        private static System.Timers.Timer _timer;
        private Message _msg;
        async public Task ExecuterComand(Message msg, ITelegramBotClient botClient, WorkerWithUserInDB dataBase)
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += RemoveMinutsUser;
            _timer.Enabled = true;
            _timer.AutoReset = true;
            _timer.Interval = 60000;
            _timer.Start();
            _sqlConnection = dataBase._sqlConnection;
            _msg = msg;
            _botClient = botClient;
            _chatId = msg.Chat.Id.ToString().Replace("-", "0");
            if (msg.Chat.Type.ToString() == "Private")
            {
                _chatId = string.Empty;
            }
            string[] massMessage = msg.Text.Split();
            massMessage[0] = massMessage[0].Remove(0, 1);
            switch (massMessage[0])
            {
                case ("иди"):
                    GoToAnyWay(massMessage);
                    break;
                case ("help"):
                    Help();
                    break;
                case ("level"):
                    Level();
                    break;
                case ("top"):
                    TopUsers();
                    break;
                case ("me"):
                    AboutUser();
                    break;
                case ("change"):
                    ChangeUser(massMessage);
                    break;
                default:
                    DefaultCommand();
                    break;
                case ("spam"):
                    Spam(massMessage);
                    break;
                case ("pvp"):
                    Pvp();
                    break;
                case ("apvp"):
                    AcceptPvp();
                    break;
                case ("attack"):
                    AttackUser();
                    break;
                case ("heal"):
                    HealthUser();
                    break;
            }
        }
        async private Task Pvp()
        {
            if (_battleFields.ContainsKey(_msg.From.Id))
            {
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, "может быть предыдущий бой закончишь?");
            }
            else
            {
                _battleFields.Add(_msg.From.Id, new Battle());
                _battleFields[_msg.From.Id].СhallengeDuel(_msg, _botClient);
                _battleWaiters.Add(_battleFields[_msg.From.Id]._secondFighter.Replace("@",""), _msg.From.Id);
                _battleFieldsTimeForDel.Add(_msg.From.Id, 5);
            }
        }
        async private Task AcceptPvp()
        {
            if (_battleWaiters.ContainsKey(_msg.From.Id.ToString()))
            {
                _battleFields[_battleWaiters[_msg.From.Id.ToString()]].AcceptDuel(_msg);
            }
            else if (_battleWaiters.ContainsKey(_msg.From.Username))
            {
                _battleWaiters.Add(_msg.From.Id.ToString(), _battleWaiters[_msg.From.Username]);
                _battleWaiters.Remove(_msg.From.Username);
                _battleFields[_battleWaiters[_msg.From.Id.ToString()]].AcceptDuel(_msg);
            }
        }
        private void AttackUser()
        {
            bool DeadOfUser = false;
            if (_battleWaiters.ContainsKey(_msg.From.Id.ToString()))
            {
                DeadOfUser = _battleFields[_battleWaiters[_msg.From.Id.ToString()]].Attack(_msg);
                _battleFieldsTimeForDel[_battleWaiters[_msg.From.Id.ToString()]] += 1;
                if (DeadOfUser == true)
                {
                    _battleFields.Remove(_battleWaiters[_msg.From.Id.ToString()]);
                    _battleWaiters.Remove(_msg.From.Id.ToString());
                }
            }
            else if (_battleFields.ContainsKey(_msg.From.Id))
            {
                DeadOfUser = _battleFields[_msg.From.Id].Attack(_msg);
                _battleFieldsTimeForDel[_msg.From.Id] += 1;
                if (DeadOfUser == true)
                {
                    _battleWaiters.Remove(_battleFields[_msg.From.Id]._secondFighter);
                    _battleFields.Remove(_msg.From.Id);
                }
            }

        }
        async private Task HealthUser()
        {
            if (_battleWaiters.ContainsKey(_msg.From.Id.ToString()))
            {
                _battleFields[_battleWaiters[_msg.From.Id.ToString()]].Heal(_msg);
                _battleFieldsTimeForDel[_battleWaiters[_msg.From.Id.ToString()]] = 10;
            }
            else if (_battleFields.ContainsKey(_msg.From.Id))
            {
                _battleFields[_msg.From.Id].Heal(_msg);
                _battleFieldsTimeForDel[_msg.From.Id] = 10;
            }
        }
        async private Task GoToAnyWay(string[] massMessage)
        {
            try
            {
                string answerBot = _msg.Text.Remove(0, (massMessage[0].Length + 2));
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, $"сам пизданул сам и иди {answerBot}", replyToMessageId: _msg.MessageId);
                _log.LogBot(_msg, answerBot);
            }
            catch
            {
                string answerBot = "сам пизданул сам и иди";
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, answerBot, replyToMessageId: _msg.MessageId);
                _log.LogBot(_msg, answerBot);
            }
        }
        async private Task DefaultCommand()
        {
            try
            {
                string answerBot = $"опять пизданул не подумав?";
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, answerBot, replyToMessageId: _msg.MessageId);
                _log.LogBot(_msg, answerBot);
            }
            catch
            {
                _log.Action("default command bot", "CommandsBot", false);
            }
        }
        async private Task TopUsers()
        {
            try
            {
                string[] userName = new string[10];
                string[] userChar = new string[10];
                _command = new SqlCommand($"SELECT TOP (10) UserInformation{_chatId}.UserName, UserTextInformation{_chatId}.UserChar FROM UserInformation{_chatId}, UserTextInformation{_chatId} WHERE UserInformation{_chatId}.UserTelegramID = UserTextInformation{_chatId}.UserTelegramID ORDER BY UserTextInformation{_chatId}.UserChar DESC", _sqlConnection);
                SqlDataReader userTextInfo = _command.ExecuteReader();
                int i = 0;
                while (userTextInfo.Read())
                {
                    userName[i] = Convert.ToString(userTextInfo.GetValue(0));
                    userChar[i] = Convert.ToString(userTextInfo.GetValue(1));
                    i++;
                }
                userTextInfo.Close();
                string answer = "";
                for (int j = 0; j < 10; j++)
                {
                    answer += $"{j + 1})@{userName[j]}|| point:{userChar[j]}\n";
                }
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, answer);
                _log.Action("Top list", "CommandsBot", true);
            }
            catch
            {
                _log.Action("send top user", "CommandsBot", false);
            }
        }
        async private Task Level()
        {
            try
            {
                _command = new SqlCommand($"SELECT Level,Exp,ExpToNextLevel FROM UserLevel{_chatId} WHERE UserTelegramID = '{_msg.From.Id}'", _sqlConnection);
                SqlDataReader userLevelInfo = _command.ExecuteReader();
                int userLevel = 0;
                int userExp = 0;
                int userExpToNextLevel = 100;
                while (userLevelInfo.Read())
                {
                    userLevel = Convert.ToInt32(userLevelInfo.GetValue(0));
                    userExp = Convert.ToInt32(userLevelInfo.GetValue(1));
                    userExpToNextLevel = Convert.ToInt32(userLevelInfo.GetValue(2));
                }
                userLevelInfo.Close();
                string answerBot = $"Уровень - {userLevel} \nУ тебя сейчас exp - {userExp} \nExp до следующего уровня - {userExpToNextLevel - userExp}";
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, answerBot, replyToMessageId: _msg.MessageId);
                _log.Action("send level user", "CommandsBot", true);
            }
            catch
            {
                _log.Action("send level user", "CommandsBot", false);
            }
        }
        async private Task AboutUser()
        {
            try
            {
                string name = "";
                string level = "";
                string prefix = "";
                string status = "";
                string numberOfChar = "";
                _command = new SqlCommand($"SELECT UserName, Prefix, Status FROM UserInformation{_chatId} WHERE UserTelegramID = '{_msg.From.Id}'", _sqlConnection);
                SqlDataReader userInfo = _command.ExecuteReader();
                while (userInfo.Read())
                {
                    name = Convert.ToString(userInfo.GetValue(0));
                    prefix = Convert.ToString(userInfo.GetValue(1));
                    status = Convert.ToString(userInfo.GetValue(2));
                }
                userInfo.Close();
                _command.CommandText = $"SELECT Level FROM UserLevel{_chatId} WHERE UserTelegramID = '{_msg.From.Id}'";
                SqlDataReader userLevel = _command.ExecuteReader();
                while (userLevel.Read())
                {
                    level = Convert.ToString(userLevel.GetValue(0));
                }
                userLevel.Close();
                _command.CommandText = $"SELECT UserChar FROM UserTextInformation{_chatId} WHERE UserTelegramID = '{_msg.From.Id}'";
                SqlDataReader userNumberOfChar = _command.ExecuteReader();
                while (userNumberOfChar.Read())
                {
                    numberOfChar = Convert.ToString(userNumberOfChar.GetValue(0));
                }
                userNumberOfChar.Close();
                string answer = $"Имя : {name}\n" +
                                $"Уровень : {level}\n" +
                                $"Статус : {status}\n" +
                                $"Префикс : {prefix}\n";
                await _botClient.SendTextMessageAsync(_msg.Chat.Id, answer, replyToMessageId: _msg.MessageId);
                _log.Action("send info about user", "CommandsBot", true);
            }
            catch
            {
                _log.Action("send info about user", "CommandsBot", false);
            }
        }
        async private Task ChangeUser(string[] massMesseng)
        {
            try
            {
                if (massMesseng.Length >= 2)
                {
                    switch (massMesseng[1])
                    {
                        case ("info"):
                            SendDefaultChangeuser();
                            _log.Action("send user info about command", "CommandsBot", true);
                            break;
                        case ("name"):
                            if (massMesseng.Length >= 3)
                            {
                                _command = new SqlCommand($"UPDATE UserInformation{_chatId} SET UserName = '{massMesseng[2]}' WHERE UserTelegramID = '{_msg.From.Id}'", _sqlConnection);
                                _command.ExecuteNonQuery();
                                _log.Action($"change user name to {massMesseng[2]}", "CommandsBot", true);
                            }
                            break;
                        case ("resname"):
                            _command = new SqlCommand($"UPDATE UserInformation{_chatId} SET UserName = '{_msg.From.Username ?? _msg.From.FirstName ?? _msg.From.LastName ?? "Ублюдок"}' WHERE UserTelegramID = '{_msg.From.Id}'", _sqlConnection);
                            _command.ExecuteNonQuery();
                            _log.Action("restart user name", "CommandsBot", true);
                            break;
                        case ("restart"):
                            _command = new SqlCommand($"UPDATE UserLevel{_chatId} SET Level = 0, Exp = 0, ExpToNextLevel = 100 WHERE UserTelegramID = '{_msg.From.Id}'", _sqlConnection);
                            _command.ExecuteNonQuery();
                            _command.CommandText = $"UPDATE UserTextInformation{_chatId} SET UserChar = 0 WHERE UserTelegramID = '{_msg.From.Id}'";
                            _command.ExecuteNonQuery();
                            _log.Action("restart user statistic", "CommandsBot", true);
                            break;
                        default:
                            SendDefaultChangeuser();
                            _log.Action("send user default option changer", "CommandsBot", true);
                            break;
                    }
                }
                else
                {
                    SendDefaultChangeuser();
                    _log.Action("end user info about command", "CommandsBot", true);
                }
            }
            catch
            {
                _log.Action("change user info", "CommandsBot", false);
            }
        }
        async private Task SendDefaultChangeuser()
        {
            await _botClient.SendTextMessageAsync(_msg.Chat.Id, "info - информация по команде \n" +
                                                                "name - изменить имя\n" +
                                                                "resname - обновить имя\n" +
                                                                "restart - сбросить статистику\n", replyToMessageId: _msg.MessageId);
        }
        async private Task Help()
        {
            string info =   "!help - информация команд бота\n" +
                            "!level - информация о уровне пользователя\n" +
                            "!me - информация о пользователе\n" +
                            "!top - топ 10 пользователей сервера\n" +
                            "!change - изменить информацию о себе\n" +
                            "!spam - отправить N-ное количество текста\n";
            await _botClient.SendTextMessageAsync(_msg.Chat.Id, info);
            _log.Action("send list commands","CommandsBot",true);
        }
        async private Task Spam(string[] massMesseng)
        {
            try
            {
                if (massMesseng.Length == 2)
                {
                    bool firstWordToInt = int.TryParse(massMesseng[1], out int valueRepeat);
                    valueRepeat = valueRepeat > 5 ? 5 : valueRepeat;
                    for (int i = 0; i < valueRepeat; i++)
                    {
                        await _botClient.SendTextMessageAsync(_msg.Chat.Id, "spam");
                    }
                    _log.Action("send spam commands", "CommandsBot", true);
                }
                else if (massMesseng.Length == 1)
                {
                    await _botClient.SendTextMessageAsync(_msg.Chat.Id, "!spam 'numberOfRepeatText' 'Text'");
                    _log.Action("send help for spam commands", "CommandsBot", true);
                }
                else
                {
                    string answer = _msg.Text.ToString().Remove(0, massMesseng[0].Length + massMesseng[1].Length + 2);
                    bool firstWordToInt = int.TryParse(massMesseng[1], out int valueRepeat);
                    valueRepeat = valueRepeat > 5 ? 5 : valueRepeat;
                    for (int i = 0; i < valueRepeat; i++)
                    {
                        await _botClient.SendTextMessageAsync(_msg.Chat.Id, answer);
                    }
                    _log.Action($"send help spam x{valueRepeat} commands", "CommandsBot", true);
                }
            }
            catch
            {
                _log.Action("send spam commands", "CommandsBot", false);
            }
        }
        async private Task RemoveBattle(long idFirst)
        {
            _battleFields.Remove(idFirst);
            foreach (string item in _battleWaiters.Keys)
            {
                if (_battleWaiters[item] == idFirst)
                {
                    _battleWaiters.Remove(item);
                }
            }
        }
        private void RemoveMinutsUser(Object source, System.Timers.ElapsedEventArgs e)
        {
            foreach (long item in _battleFieldsTimeForDel.Keys)
            {
                _battleFieldsTimeForDel[item] -= 1;
            }
            foreach (long item in _battleFieldsTimeForDel.Keys)
            {
                if (_battleFieldsTimeForDel[item] <= 0)
                {
                    RemoveBattle(item);
                    _battleFieldsTimeForDel.Remove(item);
                }
            }
        }
    }
}

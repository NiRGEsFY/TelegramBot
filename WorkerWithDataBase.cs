using System.Configuration;
using System.Data.SqlClient;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal class WorkerWithUserInDB
    {
        private readonly LogCreator logCreator = new ();
        public SqlConnection _sqlConnection;
        private SqlCommand _command;
        private LogCreator _log = new();
        public void StartWorkWithDB()
        {
            try
            {
                _sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["UserBase"].ConnectionString);
                _sqlConnection.Open();
                logCreator.Action("Connection UserDB", "WorkerWithDataBase", true);
            }
            catch
            {
                logCreator.Action("Connection UserDB","WorkerWithDataBase", false);
            }
        }
        public void WorkWithGroupLevel(Message msg)
        {
            string chatId = msg.Chat.Id.ToString().Replace("-","0");
            try
            {
                _command = new SqlCommand($"SELECT OBJECT_ID(N'UserInformation{chatId}', N'U')", _sqlConnection);
                SqlDataReader findUserInformation = _command.ExecuteReader();
                string text = "";
                while (findUserInformation.Read())
                {
                    text = Convert.ToString(findUserInformation.GetValue(0));
                }
                findUserInformation.Close();
                if (text == "")
                {
                    string chatTitle = msg.Chat.Title.ToString().Replace("'","!");
                    _command.CommandText = $"INSERT INTO TelegramChats (TelegramChatId, TelegramChatName) VALUES ({chatId}, N'{chatTitle}')";
                    _command.ExecuteNonQuery();
                    _command.CommandText = $"CREATE TABLE [dbo].[UserInformation{chatId}] (\r\n    [Id]             INT           IDENTITY (1, 1) NOT NULL,\r\n    [UserName]       NVARCHAR (50) NULL,\r\n    [UserTelegramID] NVARCHAR (50) NULL,\r\n    [Prefix]         NVARCHAR (50) NULL,\r\n    [Status]         NVARCHAR (50) NULL,\r\n    PRIMARY KEY CLUSTERED ([Id] ASC)\r\n);";
                    _command.ExecuteNonQuery();
                    _command.CommandText = $"CREATE TABLE [dbo].[UserLevel{chatId}] (\r\n    [Id]             INT           IDENTITY (1, 1) NOT NULL,\r\n    [UserTelegramID] NVARCHAR (50) NULL,\r\n    [Level]          INT           NULL,\r\n    [Exp]            INT           NULL,\r\n    [ExpToNextLevel] INT           NULL,\r\n    PRIMARY KEY CLUSTERED ([Id] ASC)\r\n);\r\n";
                    _command.ExecuteNonQuery();
                    _command.CommandText = $"CREATE TABLE [dbo].[UserTextInformation{chatId}] (\r\n    [Id]             INT           IDENTITY (1, 1) NOT NULL,\r\n    [UserTelegramID] NVARCHAR (50) NULL,\r\n    [UserTop]        INT           NULL,\r\n    [UserChar]       INT           NULL,\r\n    PRIMARY KEY CLUSTERED ([Id] ASC)\r\n);\r\n";
                    _command.ExecuteNonQuery();
                }
                
                long userTelegramId = msg.From.Id;
                _command = new SqlCommand($"SELECT * FROM UserInformation{chatId} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                findUserInformation = _command.ExecuteReader();

                if (findUserInformation.HasRows)
                {
                    findUserInformation.Close();
                }
                else
                {
                    string userName = $"{msg?.From?.Username?.Replace("'","!") ?? msg.From?.FirstName?.Replace("'", "!") ?? msg?.From?.LastName?.Replace("'", "!") ?? "NoInformation"}";
                    findUserInformation.Close();
                    _command = new SqlCommand($"INSERT INTO UserInformation{chatId} (UserName,UserTelegramID,Prefix,Status) VALUES (N'{userName}','{userTelegramId}','Wood','User')" +
                        $"INSERT INTO UserLevel{chatId} (UserTelegramID,Level,Exp,ExpToNextLevel) VALUES ('{userTelegramId}',0,0,100)" +
                        $"INSERT INTO UserTextInformation{chatId} (UserTelegramID,UserChar) VALUES ('{userTelegramId}',0)", _sqlConnection);
                    _command.ExecuteNonQuery();
                }

                string textMsg = msg.Text;
                while (textMsg.IndexOf(" ") >= 0)
                {
                    textMsg = textMsg.Replace(" ", "");
                }
                int howMachCharInText = textMsg.Length;
                int giveExp = howMachCharInText * 3;

                _command = new SqlCommand($"SELECT Level,Exp,ExpToNextLevel FROM UserLevel{chatId} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
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
                UpUserLevelGroup(userTelegramId.ToString(), userLevel, userExp, userExpToNextLevel, giveExp, chatId);

                _command = new SqlCommand($"SELECT * FROM UserTextInformation{chatId} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                SqlDataReader findUserTextInformation = _command.ExecuteReader();
                int userChar = 0;
                while (findUserTextInformation.Read())
                {
                    userChar = Convert.ToInt32(findUserTextInformation.GetValue(3));
                }
                findUserTextInformation.Close();
                UpUserCharGroup(userTelegramId.ToString(), userChar, howMachCharInText, chatId);
            }
            catch
            {
                _log.Action("Work with user DB", "WorkerWithDataBase", false);
            }
        }
        public void WorkWithLevel(Message msg)
        {
            try
            {
                long userTelegramId = msg.From.Id;
                _command = new SqlCommand($"SELECT * FROM UserInformation WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                SqlDataReader findUserInformation = _command.ExecuteReader();

                if (findUserInformation.HasRows)
                {
                    findUserInformation.Close();
                }
                else
                {
                    findUserInformation.Close();
                    string NoInformation = "No information";
                    _command = new SqlCommand($"INSERT INTO UserInformation (UserName,UserTelegramID,Prefix,Status) VALUES (N'{msg.From.Username ?? msg.From.FirstName ?? msg.From.LastName ?? NoInformation}','{userTelegramId}','Wood','User')" +
                        $"INSERT INTO UserLevel (UserTelegramID,Level,Exp,ExpToNextLevel) VALUES ('{userTelegramId}',0,0,100)" +
                        $"INSERT INTO UserTextInformation (UserTelegramID,UserChar) VALUES ('{userTelegramId}',0)", _sqlConnection);
                    _command.ExecuteNonQuery();
                }

                string textMsg = msg.Text;
                while (textMsg.IndexOf(" ") >= 0)
                {
                    textMsg = textMsg.Replace(" ", "");
                }
                int howMachCharInText = textMsg.Length;
                int giveExp = howMachCharInText * 3;

                _command = new SqlCommand($"SELECT Level,Exp,ExpToNextLevel FROM UserLevel WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
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
                UpUserLevel(userTelegramId.ToString(), userLevel, userExp, userExpToNextLevel, giveExp);

                _command = new SqlCommand($"SELECT * FROM UserTextInformation WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                SqlDataReader findUserTextInformation = _command.ExecuteReader();
                int userChar = 0;
                while (findUserTextInformation.Read())
                {
                    userChar = Convert.ToInt32(findUserTextInformation.GetValue(3));
                }
                findUserTextInformation.Close();
                UpUserChar(userTelegramId.ToString(), userChar, howMachCharInText);
            }
            catch
            {
                _log.Action("Work with user DB", "WorkerWithDataBase", false);
            }
        }
        private void UpUserLevelGroup(string userTelegramId, int level, int exp, int nextLevelExp, int giveExp, string chatId)
        {
            if ((exp + giveExp) > nextLevelExp)
            {
                level++;
                int userExp = exp + giveExp;
                exp = userExp - nextLevelExp;
                nextLevelExp = Convert.ToInt32(nextLevelExp * 1.15);
                if (exp > nextLevelExp)
                {
                    UpUserLevelGroup(userTelegramId, level, exp,nextLevelExp,0, chatId);
                }
                else
                {
                    _command = new SqlCommand($"UPDATE UserLevel{chatId} SET Level = {level}, Exp = {exp}, ExpToNextLevel = {nextLevelExp} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                    _command.ExecuteNonQuery();
                }
            }
            else
            {
                exp = exp + giveExp;
                _command = new SqlCommand($"UPDATE UserLevel{chatId} SET Exp = {exp} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                _command.ExecuteNonQuery();
            }
        }
        private void UpUserLevel(string userTelegramId, int level, int exp, int nextLevelExp, int giveExp)
        {
            if ((exp + giveExp) > nextLevelExp)
            {
                level++;
                int userExp = exp + giveExp;
                exp = userExp - nextLevelExp;
                nextLevelExp = Convert.ToInt32(nextLevelExp * 1.15);
                if (exp > nextLevelExp)
                {
                    UpUserLevel(userTelegramId, level, exp, nextLevelExp, 0);
                }
                else
                {
                    _command = new SqlCommand($"UPDATE UserLevel SET Level = {level}, Exp = {exp}, ExpToNextLevel = {nextLevelExp} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                    _command.ExecuteNonQuery();
                }
            }
            else
            {
                exp = exp + giveExp;
                _command = new SqlCommand($"UPDATE UserLevel SET Exp = {exp} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
                _command.ExecuteNonQuery();
            }
        }
        private void UpUserCharGroup(string userTelegramId, int userChar, int charInText, string chatId)
        {
            _command = new SqlCommand($"UPDATE UserTextInformation{chatId} SET UserChar = {userChar + charInText} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
            _command.ExecuteNonQuery();
        }
        private void UpUserChar(string userTelegramId, int userChar, int charInText)
        {
            _command = new SqlCommand($"UPDATE UserTextInformation SET UserChar = {userChar + charInText} WHERE UserTelegramID = '{userTelegramId}'", _sqlConnection);
            _command.ExecuteNonQuery();
        }
    }
}

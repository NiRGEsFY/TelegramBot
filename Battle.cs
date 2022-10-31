using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal class Battle
    {
        private ITelegramBotClient _botClient;


        private Random _rand = new Random();
        private bool _comingFight = false;
        private Message _firstFighterMsg;
        private Message _secondFighterMsg;
        public string _secondFighter;
        private string[] _fighters = new string[2];
        private long[] _idFighters = new long[2];
        private int[] healthFighters = new int[2];
        private int _turnAttack = 0;
        private int _turnProtect = 1;

        async public Task СhallengeDuel(Message msg, ITelegramBotClient botClient)
        {
            if (_comingFight == false && msg.Entities[0].Type == Telegram.Bot.Types.Enums.MessageEntityType.TextMention)
            {
                _botClient = botClient;
                _comingFight = true;
                _secondFighter = msg.Entities[0].User.Id.ToString();
                await _botClient.SendTextMessageAsync(msg.Chat.Id, $"Готов ли ты к анальной битве {msg.Entities[0].User.Username ?? msg.Entities[0].User.FirstName}?\n" +
                                                                    $"напиши '!apvp', если готов");
                _firstFighterMsg = msg;
            }
            else if (_comingFight == false && msg.Entities[0].Type == Telegram.Bot.Types.Enums.MessageEntityType.Mention)
            {
                _botClient = botClient;
                _comingFight = true;
                _secondFighter = msg.EntityValues.SingleOrDefault().ToString();
                AnswerBot(msg, $"Готов ли ты к анальной битве {_secondFighter}?\n" +
                               $"напиши '!apvp', если готов");
                _firstFighterMsg = msg;
            }
        }
        async public Task AcceptDuel(Message msg)
        {
            _secondFighter = msg.From.Id.ToString();
            _secondFighterMsg = msg;
            StartDuel();
        }
        async private Task StartDuel()
        {
            _fighters[0] = _firstFighterMsg?.From?.Username ?? _firstFighterMsg?.From?.FirstName ?? _firstFighterMsg?.From?.LastName ?? _firstFighterMsg?.From?.Id.ToString() ?? "NoName";
            _fighters[1] = _secondFighterMsg?.From?.Username ?? _secondFighterMsg?.From?.FirstName ?? _secondFighterMsg?.From?.LastName ?? _secondFighterMsg?.From?.Id.ToString() ?? "NoName";
            _idFighters[0] = _firstFighterMsg.From.Id;
            _idFighters[1] = _secondFighterMsg.From.Id;
            healthFighters[0] = 100;
            healthFighters[1] = 100;
            int firstAttack = _rand.Next(0, 2);
            _turnAttack = firstAttack;
            if (_turnAttack == 1)
            {
                _turnProtect = 0;
            }
            else
            {
                _turnProtect = 1;
            }
            AnswerBot(_firstFighterMsg, $"Дорогие зрители на данной арене сразятся два до селе не виданых долбаеба\n" +
                                        $"Встречайте первого анального бойца {_fighters[0]}\n" +
                                        $"А его соперником станет {_fighters[1]} \n" +
                                        $"И первым начнет {_fighters[firstAttack]}\n" +
                                        $"Чтобы атаковать противника напиши !attack\n");
        }
        public bool Attack(Message msg)
        {
            if (msg.From.Id == _idFighters[_turnAttack])
            {
                string critText = "атаковал";
                double attack = _rand.Next(10, 30);
                double crit = _rand.Next(0, 100);
                double miss = _rand.Next(0, 100);
                if (crit >= 60 && crit <= 75)
                {
                    critText = "пнул";
                    attack *= 1.25;
                }
                else if (crit > 75 && crit <= 90)
                {
                    critText = "уебал";
                    attack *= 1.5;
                }
                else if (crit > 90)
                {
                    critText = "кританул";
                    attack *= 2;
                }
                if (miss >= 85)
                {
                    AnswerBot(_firstFighterMsg, $"{_fighters[_turnAttack]} миссанул");
                    Reverse();
                }
                else
                {
                    healthFighters[_turnProtect] -= Convert.ToInt32(attack);
                    AnswerBot(_firstFighterMsg, $"{_fighters[_turnAttack]} так {critText} что {_fighters[_turnProtect]} потерял {Convert.ToInt32(attack)} HP\n" +
                                                $"{_fighters[_turnAttack]} HP: {healthFighters[_turnAttack]}|{_fighters[_turnProtect]} HP: {healthFighters[_turnProtect]}");
                    if (healthFighters[_turnProtect] < 0)
                    {
                        Win();
                        return true;
                    }
                    Reverse();
                    return false;
                }
            }
            else
            {
                AnswerBot(_firstFighterMsg, "Ублюдок жди очередь");
            }
            return false;
        }
        async public Task Heal(Message msg)
        {
            if (msg.From.Id == _idFighters[_turnAttack])
            {
                string critText = "Попил водичьки";
                double heal = _rand.Next(5, 25);
                double crit = _rand.Next(0, 100);
                if (crit >= 0 && crit <= 10)
                {
                    critText = "Подскользнулся и упал";
                    heal *= -0.25;
                }
                else if (crit >= 50 && crit <= 65)
                {
                    critText = "Перекусил";
                    heal *= 1.1;
                }
                else if (crit > 65 && crit <= 80)
                {
                    critText = "Выпил зелье";
                    heal *= 1.25;
                }
                else if (crit > 80)
                {
                    critText = "Использовал аптечку";
                    heal *= 1.5;
                }
                else if (crit > 99)
                {
                    critText = "Бахнул пива";
                    heal *= 4;
                }
                healthFighters[_turnAttack] += Convert.ToInt32(heal);
                AnswerBot(_firstFighterMsg, $"{_fighters[_turnAttack]} {critText} и получил {Convert.ToInt32(heal)} HP\n" +
                                            $"{_fighters[_turnAttack]} HP: {healthFighters[_turnAttack]}|{_fighters[_turnProtect]} HP: {healthFighters[_turnProtect]}");


                Reverse();
            }
            else
            {
                await _botClient.SendTextMessageAsync(msg.Chat.Id, "Ублюдок жди очередь");
            }
        }
        private async Task Win()
        {
            await _botClient.SendTextMessageAsync(_firstFighterMsg.Chat.Id, $"Победил {_fighters[_turnAttack]}");
        }

        private async Task AnswerBot(Message msg, string answer)
        {
            await _botClient.SendTextMessageAsync(msg.Chat.Id, answer);
        }
        private void Reverse()
        {
            int switcher = _turnProtect;
            _turnProtect = _turnAttack;
            _turnAttack = switcher;
        }
    }
}

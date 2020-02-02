using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaqBot.Model;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FaqBot
{
    public static class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("");

        static void Main(string[] args)
        {
            var bot = Bot.GetMeAsync().Result;
            Console.Title = bot.Username;

            Bot.OnMessage += BotOnMessageReceived2;
            Bot.OnMessageEdited += BotOnMessageReceived2;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{ bot.Username }");
            Console.ReadLine();
        }

        private static async void BotOnMessageReceived2(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commandsJson = await File.ReadAllTextAsync(currentDirectory + "/BotCommands.json");
            var commands = JsonConvert.DeserializeObject<BotCommands>(commandsJson);

            ReplyKeyboardMarkup startKeyboard = new[]
            {
                new [] {"Задать вопрос"},
                new [] {"Часто задаваемые вопросы"},
                new [] {"Инфо о боте"}
            };

            if (message.Text == "/start" || message.Text == "На главную")
            {
                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Выберите",
                    replyMarkup: startKeyboard);
                return;
            }

            if (message.Text == "Задать вопрос")
            {

                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Какой вопрос Вас интересует?",
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            #region FAQ

            if (message.Text == "Часто задаваемые вопросы")
            {
                var keyboardButtons = new List<List<KeyboardButton>>();
                foreach (var command in commands.Commands)
                {
                    KeyboardButton kButton = new KeyboardButton(command.Question);
                    keyboardButtons.Add(new List<KeyboardButton> { kButton });
                }

                ReplyKeyboardMarkup faqKeyboard = new ReplyKeyboardMarkup(keyboardButtons);

                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Выберите",
                    replyMarkup: faqKeyboard);
                return;
            }

            #endregion

            #region UserQuestion
            
            var userQuestion = message.Text.ToLower();

            var listOfSimilarQuestions = new List<Command>();

            foreach (var command in commands.Commands)
            {
                if (command.Question.ToLower().Contains(userQuestion))
                {
                    listOfSimilarQuestions.Add(command);
                }

                foreach (var shortcut in command.Shortcuts)
                {
                    if (userQuestion.Contains(shortcut))
                    {
                        listOfSimilarQuestions.Add(command);
                    }
                }
            }

            var distSimilarQuestions = listOfSimilarQuestions.GroupBy(x => x.Question).Select(x => x.First()).ToList();

            if (distSimilarQuestions.Count > 1)
            {
                var similarQuestionsButtons = new List<List<KeyboardButton>>();
                foreach (var similarQuestion in distSimilarQuestions) 
                {
                    KeyboardButton kButton = new KeyboardButton(similarQuestion.Question);
                    similarQuestionsButtons.Add(new List<KeyboardButton> { kButton });
                }
                ReplyKeyboardMarkup similarQuestionsKeyboard = new ReplyKeyboardMarkup(similarQuestionsButtons);

                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Есть несколько подходящих вопросов. Выберите один",
                    replyMarkup: similarQuestionsKeyboard);
                return;
            }

            if (distSimilarQuestions.Count == 1)
            {
                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    distSimilarQuestions.FirstOrDefault().Answer,
                    replyMarkup: startKeyboard);
                return;
            }

            #endregion

            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "К сожалению по Вашему запросу ничего не найдено. Попробуйте посмотреть список часто задаваемых вопросов. Также Вы можете обратиться в Центр Заботы ** ******, набрав с мобильного телефона короткий номер 330 (звонок бесплатный) или с городского телефона +77172******",
                replyMarkup: startKeyboard);

            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выберите",
                replyMarkup: startKeyboard);
        }

    }
}

using Croc.TestDrive.TgChatBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Croc.TestDrive.TgChatBot
{
	/// <summary>
	/// Создаётся новый экземпляр на каждое сообщение
	/// </summary>
	internal class BotMessageHandler
	{
		private readonly IUserContext _userContext;

		/// <summary>
		/// Если что-то понадобится добавить через DI
		/// Скорее всего потребуется сервис для хранения истории сообщений
		/// Ну или можно сделать в статической переменной
		/// </summary>
		public BotMessageHandler(IUserContext userContext)
		{
			_userContext = userContext;
		}

		/// <summary>
		/// Тут бот получает сообщения от пользователей, нужно считать или все входные данные сразу и выдать ответ
		/// Или считывать данные потихоньку запоминая всё например в словарь с состоянием и ключём chatId
		/// Стоит учесть что <see cref="BotMessageHandler"/> сейчас создаётся новый на каждое сообщение поэтому 
		/// или это нужно поменять в классе <see cref="Program"/> 
		/// или контроллер состояния, одиночку, через конструктор
		/// или сделать статическую переменную например
		/// Документация: https://telegrambots.github.io/book/
		/// </summary>
		/// <param name="telegramBot"></param>
		/// <param name="update"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task OnInputAsync(ITelegramBotClient telegramBot, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == UpdateType.CallbackQuery)
			{
				var callback = update.CallbackQuery!;
				if (callback.Message is null) return;
				var callbackChat = callback.Message.Chat.Id;
				var callbackContext = await _userContext.GetContext(callbackChat, telegramBot);
				if (callback.Data == nameof(IContext.Interrupt))
				{
					await callbackContext.Interrupt();
					await telegramBot.AnswerCallbackQueryAsync(callback.Id);
					return;
				}
				if (callback.Data == nameof(IContext.Reset))
				{
					await callbackContext.Reset();
					await telegramBot.SendTextMessageAsync(callbackChat, "О чём поговорим?");
					await telegramBot.AnswerCallbackQueryAsync(callback.Id);
					return;
				}
			}

			if (update.Type != UpdateType.Message || update.Message is null || update.Message.From is null) return;
			var message = update.Message;
			var messageText = message.Text;
			if (string.IsNullOrWhiteSpace(messageText)) return;
			var from = message.From;
			var firstName = from.FirstName ?? "Неизвестный";
			var userId = from.Id;
			var chatId = message.Chat.Id;
			RunBackgound(telegramBot, messageText, chatId);
			await telegramBot.SendTextMessageAsync(chatId, "Ответ подготавливается");
			async void RunBackgound(ITelegramBotClient telegramBot, string messageText, long chatId)
			{
				try
				{
					await Task.Run(async () =>
					{
						var context = await _userContext.GetContext(chatId, telegramBot);
						await context.Ask(messageText);
					});
				}
				catch
				{

				}
			}
		}
	}
}

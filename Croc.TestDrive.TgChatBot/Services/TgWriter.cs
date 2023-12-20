using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Croc.TestDrive.TgChatBot.Services
{
	internal class TgWriter : IContextListener
	{
		private readonly long _chatId;
		private readonly ITelegramBotClient _telegramBotClient;
		private readonly StringBuilder _message = new StringBuilder();
		private static DateTime _lastSent = DateTime.MinValue;
		private static TimeSpan _delay = TimeSpan.FromSeconds(1);
		private int? _messageId = null;
		private TgWriter(long chatId, ITelegramBotClient telegramBotClient)
		{
			_chatId = chatId;
			_telegramBotClient = telegramBotClient;
		}

		public static Task<IContextListener> From(long chatId, ITelegramBotClient telegramBotClient)
		{
			IContextListener tg = new TgWriter(chatId, telegramBotClient);
			return Task.FromResult(tg);
		}

		public async Task Write(string text)
		{
			if (string.IsNullOrEmpty(text) || (_message.Length == 0 && string.IsNullOrWhiteSpace(text)))
			{
				return;
			}

			_message.Append(text);
			if (DateTime.Now - _lastSent > _delay)
			{
				_lastSent = DateTime.Now;
				await Send(false);
			}
		}

		public async Task Flush()
		{
			await Send(true);
			_message.Clear();
			_messageId = null;
		}

		private async Task Send(bool finish = false)
		{
			InlineKeyboardMarkup streaming = new(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("Остановить", nameof(IContext.Interrupt)) },
			});
			InlineKeyboardMarkup mainMenu = new(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("Начать новую тему", nameof(IContext.Reset)) },
			});

			var replyMarkup = finish ? mainMenu : streaming;

			var text = _message.ToString();

			if (string.IsNullOrWhiteSpace(text)) return;

			if (_messageId.HasValue)
			{
				await _telegramBotClient.EditMessageTextAsync(_chatId, _messageId.Value, text, replyMarkup: replyMarkup);

				if (_message.Length > 2048)
				{
					_message.Clear();
					_messageId = null;
				}

			}
			else
			{
				var msg = await _telegramBotClient.SendTextMessageAsync(_chatId, text, replyMarkup: replyMarkup);
				_messageId = msg.MessageId;
			}
		}
	}
}

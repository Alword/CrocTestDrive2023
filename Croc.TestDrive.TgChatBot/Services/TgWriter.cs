using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Croc.TestDrive.TgChatBot.Services
{
	internal class TgWriter : IContextListener
	{
		private const string _initMessage = "Поиск...";
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

		public static async Task<IContextListener> From(long chatId, ITelegramBotClient telegramBotClient)
		{
			var tg = new TgWriter(chatId, telegramBotClient);
			return tg;
		}

		public async Task Write(string? text, bool isFinish = false)
		{
			if (text is null)
			{
				await Write(_initMessage);
				return;
			}

			if (_message.Length == _initMessage.Length && _message.ToString() == _initMessage) _message.Clear();
			_message.Append(text);
			if (DateTime.Now - _lastSent > _delay)
			{
				_lastSent = DateTime.Now;
				await Write(isFinish);
			}
		}

		public async Task Flush(string? text = null)
		{
			if (text is not null)
			{
				await Write(text, true);
			}
			else
			{
				await Write(true);
			}
			_message.Clear();
			_messageId = null;
		}

		private async Task Write(bool finish = false)
		{
			InlineKeyboardMarkup streaming = new InlineKeyboardMarkup(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("Остановить", nameof(IContext.Interrupt)) },
			});
			InlineKeyboardMarkup mainMenu = new InlineKeyboardMarkup(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("Новый чат", nameof(IContext.Reset)) },
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

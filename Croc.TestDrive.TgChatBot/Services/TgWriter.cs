using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
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
		private Message? _msg = null;
		private int _lastMessageSize = 0;
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
			_msg = null;
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
			_lastMessageSize = text.Length;

			if (_msg is not null)
			{
				if (text.Trim() == _msg.Text)
				{
					if (finish) text += " 🧀";
					else return;
				}
				_msg = await _telegramBotClient.EditMessageTextAsync(_chatId, _msg.MessageId, text, replyMarkup: replyMarkup);

				if (_message.Length > 2048)
				{
					_message.Clear();
					_msg = null;
				}

			}
			else
			{
				_msg = await _telegramBotClient.SendTextMessageAsync(_chatId, text, replyMarkup: replyMarkup);
			}
		}

		public string Response() => _message.ToString();
	}
}

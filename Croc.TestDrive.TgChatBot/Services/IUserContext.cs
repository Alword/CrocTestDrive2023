using Telegram.Bot;

namespace Croc.TestDrive.TgChatBot.Services
{
	public interface IUserContext
	{
		public Task<IContext> GetContext(long userId, ITelegramBotClient telegramBotClient);
	}
}

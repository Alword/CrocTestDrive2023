using Forge.OpenAI.Interfaces.Services;
using Telegram.Bot;

namespace Croc.TestDrive.TgChatBot.Services
{

	internal class UserContextService : IUserContext
	{
		private const int _contextSize = 1024 * 32;
		public readonly Dictionary<long, IContext> _context = new Dictionary<long, IContext>();
		private readonly IOpenAIService _openAIService;
		public UserContextService(IOpenAIService openAIService)
		{
			_openAIService = openAIService;
		}

		public async Task<IContext> GetContext(long chatId, ITelegramBotClient telegramBotClient)
		{
			if (!_context.TryGetValue(chatId, out var context))
			{
				var writer = await TgWriter.From(chatId, telegramBotClient);
				context = new UserContext(writer, _openAIService);
				_context[chatId] = context;
			}
			return context;
		}
	}
}

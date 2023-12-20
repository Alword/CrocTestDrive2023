using Telegram.Bot;

namespace Croc.TestDrive.TgChatBot.Services
{

	internal class UserContextService : IUserContext
	{
		public readonly Dictionary<long, IContext> _context = new Dictionary<long, IContext>();
		private readonly OllamaApiClient _ollama;
		public UserContextService()
		{
			var uri = new Uri("http://localhost:11434");
			_ollama = new OllamaApiClient(uri);
		}

		public async Task<IContext> GetContext(long chatId, ITelegramBotClient telegramBotClient)
		{
			if (!_context.TryGetValue(chatId, out var context))
			{
				var writer = await TgWriter.From(chatId, telegramBotClient);
				context = new LamaContext(writer, _ollama);
				_context[chatId] = context;
			}
			return context;
		}
	}
}

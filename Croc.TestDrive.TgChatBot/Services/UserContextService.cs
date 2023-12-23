using LLama;
using LLama.Common;
using Telegram.Bot;

namespace Croc.TestDrive.TgChatBot.Services
{

	internal class UserContextService : IUserContext
	{
		private const int _contextSize = 1024 * 32;
		private const string _contextPath = "/opt/gguf/openbuddy-mistral-7b-v13.Q8_0.gguf";
		public readonly Dictionary<long, IContext> _context = new Dictionary<long, IContext>();
		private readonly LLamaWeights _model;
		private readonly ModelParams _params;
		public UserContextService()
		{
			string modelPath = _contextPath;
			this._params = new ModelParams(modelPath) { ContextSize = _contextSize };
			this._model = LLamaWeights.LoadFromFile(this._params);
		}

		public async Task<IContext> GetContext(long chatId, ITelegramBotClient telegramBotClient)
		{
			if (!_context.TryGetValue(chatId, out var context))
			{
				var writer = await TgWriter.From(chatId, telegramBotClient);
				context = new UserContext(writer, _model, _params);
				_context[chatId] = context;
			}
			return context;
		}
	}
}

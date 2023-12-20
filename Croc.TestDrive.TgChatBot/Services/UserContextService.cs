using Telegram.Bot;

namespace Croc.TestDrive.TgChatBot.Services
{
	public interface IContextListener
	{
		public Task Write(string text);
		public Task Flush();
	}

	public interface IContext
	{
		public Task Ask(string message);
		public Task Interrupt();
		public Task Reset();
		public Task SetListener(IContextListener listener);
	}

	public class LamaContext : IContext
	{
		private static readonly string _model = "dolphin-mixtral:latest";
		private readonly IContextListener _listener;
		private readonly OllamaApiClient _client;
		private ConversationContext? _context;
		public LamaContext(
			IContextListener listener,
			OllamaApiClient client)
		{
			_listener = listener;
			_client = client;
		}
		public async Task Ask(string message)
		{
			try
			{
				_context = await _client.StreamCompletion($"[Отвечай на русском] {message}", _model, _context, stream => _listener.Write(stream.Response));
			}
			finally
			{
				await _listener.Flush();
			}
		}

		public async Task Interrupt()
		{
			throw new NotImplementedException();
		}

		public Task Reset()
		{
			_context = null;
			_listener.Write("Диалог очищен");
			_listener.Flush();
			return Task.CompletedTask;
		}

		public Task SetListener(IContextListener listener)
		{
			throw new NotImplementedException();
		}
	}

	public interface IUserContext
	{
		public Task<IContext> GetContext(long userId, ITelegramBotClient telegramBotClient);
	}

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
			}
			return context;
		}
	}
}

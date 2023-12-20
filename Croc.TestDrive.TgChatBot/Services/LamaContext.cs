namespace Croc.TestDrive.TgChatBot.Services
{
	public class LamaContext : IContext
	{
		private static readonly string _model = "dolphin-mixtral:latest";
		private readonly IContextListener _listener;
		private readonly OllamaApiClient _client;
		private ConversationContext? _context;
		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		public LamaContext(
			IContextListener listener,
			OllamaApiClient client)
		{
			_listener = listener;
			_client = client;
		}
		public async Task Ask(string message)
		{
			_tokenSource.Cancel();
			_tokenSource = new CancellationTokenSource();
			try
			{
				_context = await _client.StreamCompletion(
					$"[Отвечай на русском] {message}",
					_model,
					_context, stream => _listener.Write(stream.Response),
					_tokenSource.Token
				);
			}
			finally
			{
				await _listener.Flush();
			}
		}

		public Task Interrupt()
		{
			_tokenSource.Cancel();
			return Task.CompletedTask;
		}

		public Task Reset()
		{
			_context = null;
			return Task.CompletedTask;
		}
	}
}



using Forge.OpenAI.Interfaces.Services;
using Forge.OpenAI.Models.ChatCompletions;

namespace Croc.TestDrive.TgChatBot.Services
{
	public class UserContext : IContext
	{
		private static readonly string _defaultModel = "guanaco-13b-uncensored.Q5_K_M.gguf";
		private readonly IContextListener _listener;
		private readonly IOpenAIService _openAIService;
		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private ChatCompletionRequest? _chat;
		public UserContext(IContextListener listener, IOpenAIService openAIService)
		{
			_listener = listener;
			_openAIService = openAIService;
		}
		public async Task Ask(string message)
		{

			if (_chat is null)
			{
				_chat = CreateSession(message);
			}
			else
			{
				_chat.Messages.Add(ChatMessage.CreateFromUser(message));
			}

			try
			{
				_tokenSource = new CancellationTokenSource();
				await _listener.Init();

				await foreach (var response in _openAIService.ChatCompletionService.GetStreamAsync(_chat, _tokenSource.Token))
				{
					await _listener.Write(response.Result?.Choices[0].Delta.Content ?? string.Empty);
				}
				_chat.Messages.Add(ChatMessage.CreateFromAssistant(_listener.Response()));
			}
			finally
			{
				await _listener.Flush();
				_tokenSource.Cancel();
				GC.Collect();
			}
		}

		public Task Interrupt()
		{
			_tokenSource.Cancel();
			return Task.CompletedTask;
		}

		public Task Reset()
		{
			_chat = null;
			return Task.CompletedTask;
		}

		private ChatCompletionRequest CreateSession(string message)
		{
			var chat = new ChatCompletionRequest(
					ChatMessage.CreateFromUser(message),
					_defaultModel
				)
			{
				MaxTokens = 4096,
				Temperature = 0.1, // lower value means more precise answer
				NumberOfChoicesPerMessage = 1,
			};
			chat.Messages.Add(ChatMessage.CreateFromSystem("Используй русский языке"));
			return chat;
		}
	}
}

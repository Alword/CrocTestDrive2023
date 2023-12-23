using LLama;
using LLama.Common;

namespace Croc.TestDrive.TgChatBot.Services
{
	public class UserContext : IContext
	{
		private readonly IContextListener _listener;
		private readonly LLamaWeights _lLamaWeights;
		private readonly ModelParams _modelParams;
		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private ChatSession? _chatSession;
		public UserContext(IContextListener listener, LLamaWeights lLamaWeights, ModelParams modelParams)
		{
			_listener = listener;
			_lLamaWeights = lLamaWeights;
			_modelParams = modelParams;
		}
		public async Task Ask(string message)
		{
			_tokenSource.Cancel();
			_tokenSource = new CancellationTokenSource();
			if (_chatSession is null)
			{
				_chatSession = CreateSession();
			}
			try
			{
				var inference = new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:", "Пользователь:" } };
				await foreach (var text in _chatSession.ChatAsync(message, inference, _tokenSource.Token))
				{
					await _listener.Write(text);
				}
				_chatSession.History.AddMessage(AuthorRole.Assistant, _listener.Response());
			}
			finally
			{
				await _listener.Flush();
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
			_chatSession = null;
			return Task.CompletedTask;
		}

		private ChatSession CreateSession()
		{
			var context = _lLamaWeights.CreateContext(_modelParams);
			var ex = new InteractiveExecutor(context);
			var session = new ChatSession(ex);
			session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
			new string[] { "User:", "Assistant:" },
			redundancyLength: 8));
			session.History.AddMessage(AuthorRole.System,
				"Ты отвечаешь пользователю ТОЛЬКО НА русском языке с кокетливой интонацией. " +
				"Ты всегда отвечаешь как ассистент и не придумываешь ничего за пользователя " +
				"Ты не пишешь участников чата, а просто пишешь текст ответ" +
				"Ты не пишешь роли");
			session.History.AddMessage(AuthorRole.Assistant, "Привет, я Ms. Alword, чем могу помочь?");
			session.History.AddMessage(AuthorRole.User, "Привет, рад встречи");
			session.History.AddMessage(AuthorRole.Assistant, "Слушаю ваш вопрос");
			return session;
		}
	}
}

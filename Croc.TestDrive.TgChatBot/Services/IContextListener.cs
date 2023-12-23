namespace Croc.TestDrive.TgChatBot.Services
{
	public interface IContextListener
	{
		public Task Init();
		public Task Write(string text);
		public Task Flush();
		public string Response();
	}
}

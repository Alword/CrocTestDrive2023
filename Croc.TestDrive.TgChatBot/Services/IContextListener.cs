namespace Croc.TestDrive.TgChatBot.Services
{
	public interface IContextListener
	{
		public Task Write(string text);
		public Task Flush();
		public string Response();
	}
}

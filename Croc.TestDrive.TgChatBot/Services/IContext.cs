namespace Croc.TestDrive.TgChatBot.Services
{
	public interface IContext
	{
		public Task Ask(string message);
		public Task Interrupt();
		public Task Reset();
	}
}

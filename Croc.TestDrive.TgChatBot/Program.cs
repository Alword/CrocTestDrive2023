using Croc.TestDrive.TgChatBot;
using Croc.TestDrive.TgChatBot.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Нужно получить токен у https://telegram.me/BotFather</summary>

class Program
{
	const string TOKEN = "6769496345:AAEkLuwkf0i3_jlLW06atkRUCiKCVxb5cbU";
	// const string TOKEN = "6665853854:AAHQJEHHrdIq7pEkunrFr6G18XxmDQsEemA";
	static async Task Main(string[] args)
	{
		var serviceProvider = ConfigureServices();
		var bot = serviceProvider.GetRequiredService<TelegramBot>();
		await bot.StartAsync();
	}
	static IServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();
		services.AddSingleton(sp => new TelegramBot(TOKEN, sp));
		services.AddSingleton<IUserContext, UserContextService>();
		// Тот самый класс который всё слушает BotMessageHandler
		services.AddTransient<BotMessageHandler>();
		// Другие сервисы и зависимости могут быть добавлены сюда
		return services.BuildServiceProvider();
	}
}



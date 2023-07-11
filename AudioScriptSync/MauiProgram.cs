
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace AudioScriptSync;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageModel>();


        return builder.Build();
	}
}


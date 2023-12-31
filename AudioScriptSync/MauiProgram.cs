﻿
using Microsoft.Extensions.Configuration;
using System.Reflection;
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

        //
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("AudioScriptSync.appsettings.json");

        var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
        builder.Configuration.AddConfiguration(config);


        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageModel>();
        builder.Services.AddTransient<ArticleEditPage>();
        builder.Services.AddTransient<ArticleEditPageModel>();


        return builder.Build();
	}
}


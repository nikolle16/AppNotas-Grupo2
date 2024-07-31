using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace App_Notas___Grupo_2
{
    public static class MauiProgram
    {
        private static IServiceProvider _serviceProvider;

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

            builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);
            builder.Services.AddTransient<Views.AggNotaAudio>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            _serviceProvider = app.Services;

            return app;
        }

        public static T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }
    }
}


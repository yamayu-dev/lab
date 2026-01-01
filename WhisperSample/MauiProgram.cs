using Microsoft.Extensions.Logging;
using WhisperSample.Services;
using WhisperSample.Services.Audio;
using WhisperSample.Services.Whisper;

namespace WhisperSample
{
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

            builder.Services.AddSingleton<WhisperModelService>();
            builder.Services.AddSingleton<WhisperTranscriptionService>();
            builder.Services.AddSingleton<RealtimeTranscriber>();

            builder.Services.AddTransient<MainPage>();

            // iOSでは Services.Audio.AudioStreamSource（.ios.cs）を、それ以外はUnsupportedを使う
            builder.Services.AddTransient<IAudioStreamSource>(sp =>
            {
                var iosType = Type.GetType("WhisperSample.Services.Audio.AudioStreamSource, WhisperSample", throwOnError: false);
                if (iosType != null)
                    return (IAudioStreamSource)ActivatorUtilities.CreateInstance(sp, iosType);

                return ActivatorUtilities.CreateInstance<UnsupportedAudioStreamSource>(sp);
            });
            builder.Services.AddTransient<Func<IAudioStreamSource>>(sp => () => sp.GetRequiredService<IAudioStreamSource>());

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

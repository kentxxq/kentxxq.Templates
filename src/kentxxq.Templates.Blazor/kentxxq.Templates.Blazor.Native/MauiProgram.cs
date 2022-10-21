using kentxxq.Templates.Blazor.Native.Services;
using kentxxq.Templates.Blazor.UI.Interfaces;

namespace kentxxq.Templates.Blazor.Native
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
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            // 自己的服务配置
            builder.Services.AddSingleton<ITextService, TextService>();

            return builder.Build();
        }
    }
}
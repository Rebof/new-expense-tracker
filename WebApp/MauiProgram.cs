using Microsoft.Extensions.Logging;
using Radzen;
using WebApp.Services;
using WebApp.Services.Interface;


namespace WebApp
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
            //Register services 
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>(); 



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Services.AddRadzenComponents();
			builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

using Microsoft.AspNetCore.Components.WebView.Maui;
using Neopixel.Client;
using NeopixelControl.Data;
using NeopixelControl.Shared;

namespace NeopixelControl;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        
        builder.Services.AddScoped<NeopixelClient>(_ => new("192.168.1.157"));

        return builder.Build();
    }
}
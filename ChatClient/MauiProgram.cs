using LoggerLibrary;
using Microsoft.Extensions.Logging;

/// <summary>
/// Author:    Catherine Bao
/// Partner:   Jessie Taubert
/// Date:      April 1, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Catherine Bao and Jessie Taubert - This work 
///            may not be copied for use in Academic Coursework.
///
/// I, Catherine Bao and Jessie Taubert, certify that I wrote this 
/// code from scratch and did not copy it in part or whole from 
/// another source.  All references used in the completion of the 
/// assignments are cited in my README file.
///
/// File Contents
/// This project contains the GUi for the chat client which allows a user to 
/// connect to the server, receive messages, and disconnect from the server.
/// </summary>
/// 
namespace ChatClient
{
    /// <summary>
    /// The main Maui app for the client, DI for logger
    /// </summary>
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

            builder.Services.AddLogging(configure =>
        {
            configure.AddDebug();
            configure.AddProvider(new CustomFileLoggerProvider());
            configure.SetMinimumLevel(LogLevel.Trace);
        }).AddTransient<MainPage>();
            ;

            return builder.Build();
        }
    }
}
using LibraryApp.Infrastructure;
using LibraryApp.UI.ViewModels;
using LibraryApp.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using LibraryApp.Application;
using LibraryApp.Infrastructure.Data;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LibraryApp;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        // 1. Build configuration from appsettings.json file
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Define the static log file path
        const string logFilePath = "logs/import.log";

        // 3. Delete the previous log file if it exists to ensure a fresh start
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // 4. Initialize Serilog using JSON configuration settings
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File(
                path: "logs/import.log",
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _serviceProvider = ConfigureServices().BuildServiceProvider();
    }

    /// <summary>Registers all application services into the DI container.</summary>
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddSerilog(dispose: true));
        services.AddInfrastructure("Data Source=library.db");
        services.AddApplication();

        // Main window setup
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowVm>();

        // Import window — factory allows creating a fresh instance per import session
        services.AddTransient<ImportWindow>();
        services.AddTransient<ImportWindowVm>();
        services.AddSingleton<Func<ImportWindow>>(sp => sp.GetRequiredService<ImportWindow>);

        return services;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            db.Database.Migrate();
        }

        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }
}

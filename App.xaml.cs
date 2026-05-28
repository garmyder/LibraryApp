using LibraryApp.Infrastructure;
using LibraryApp.UI.ViewModels;
using LibraryApp.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using LibraryApp.Application;
using LibraryApp.Infrastructure.Data;
using Serilog;

namespace LibraryApp;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
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

        // Main window
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        // Import window — factory allows creating a fresh instance per import session
        services.AddTransient<ImportWindow>();
        services.AddTransient<ImportWindowViewModel>();
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

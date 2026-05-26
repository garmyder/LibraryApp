// App.xaml.cs
using LibraryApp.Infrastructure;
using LibraryApp.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using LibraryApp.Application;
using LibraryApp.Infrastructure.Data;
using LibraryApp.UI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        _serviceProvider = ConfigureServices().BuildServiceProvider();
    }

    /// <summary>Registers all application services into the DI container.</summary>
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure("Data Source=library.db");
        services.AddApplication();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        return services;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure DB is created and all migrations are applied
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            db.Database.Migrate();
        }

        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }
}
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensorDrive;
using SensorUI.Service;
using SensorUI.ViewModels;
using SensorUI.Views;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SensorUI
{
    internal class Program
    {
        /*
            Код инициализации. Не используйте Avalonia, сторонние API или любые
            Синхронизация Контекстно-зависимый код перед вызовом App Main: вещи не инициализируются
            еще и вещи могут сломаться.
        */

        [STAThread]
        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .CreateLogger();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddTransient<DeviceSettingsViewModel>();
                    services.AddSingleton(s => new MainWindow
                    {
                        DataContext = s.GetRequiredService<MainWindowViewModel>()
                    });
                    services.AddSingleton<IDeviceService,DeviceService>();
                    services.AddSingleton<IDriver, FakeDriver>();
                })
                .UseSerilog()
                .Build();

            host.Start();

            _ = BuildAvaloniaApp(host.Services)
                .StartWithClassicDesktopLifetime(args);

            await host.StopAsync();
            host.Dispose();
        }

        // Конфигурация Avalonia, не удалять; используется для старта приложения с внедрением зависимостей.
        public static AppBuilder BuildAvaloniaApp(IServiceProvider service)
        {
            return AppBuilder.Configure(() => new App(service))
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace()
                        .UseReactiveUI();
        }

        // Конфигурация Avalonia, не удалять; также используется визуальным дизайнером.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();


        private static void BuildConfig(IConfigurationBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_MODIFIABLE_ASSEMBLIES");
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env ?? "release"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}
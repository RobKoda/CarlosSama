using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using Fazbot.App.Services;
using Fazbot.AudioPlayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fazbot.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets(typeof(App).GetTypeInfo().Assembly);
        var servicesCollection = new ServiceCollection();
        servicesCollection.AddSingleton<IConfiguration>(_ => builder.Build());

        ConfigureServices(servicesCollection);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ServiceProvider = servicesCollection.BuildServiceProvider();
        
        var discordBot = ServiceProvider.GetRequiredService<DiscordBotService>();
        Task.Factory.StartNew(() => discordBot.StartBotAsync());
        
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<CustomAudioPlayer>();
        
        services.AddSingleton<FazComputer>();
        services.AddSingleton<AdminCommandsService>();
        services.AddSingleton<FazComputerMessagesService>();
        services.AddSingleton<AdminMessagesService>();
        services.AddSingleton<MessagesService>();
        services.AddSingleton<FazbotMessageHandler>();
        services.AddSingleton<DiscordBotService>();
        
        services.AddTransient<MainWindow>();
    }
}
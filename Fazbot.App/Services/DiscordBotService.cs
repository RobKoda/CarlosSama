using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Configuration;

namespace Fazbot.App.Services;

public class DiscordBotService
{
    private readonly DiscordClient _client;
    
    public DiscordBotService(FazbotMessageHandler messageHandler, IConfiguration configuration)
    {
        var token = configuration["DiscordToken"]!;
        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.All);
        builder.ConfigureEventHandlers(configure => 
            configure.HandleMessageCreated((client, args) => messageHandler.OnMessageCreated(client, args, this)));
        builder.UseVoiceNext(new VoiceNextConfiguration());
        _client = builder.Build();
    }
    
    public async Task StartBotAsync()
    {
        await _client.ConnectAsync();
        await Task.Delay(-1);
    }

    public DiscordGuild GetGuild() => _client.Guilds.Single().Value;
    public DiscordClient GetClient() => _client;
}
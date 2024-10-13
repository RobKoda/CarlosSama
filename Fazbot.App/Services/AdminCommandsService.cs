using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Fazbot.AudioPlayer;

namespace Fazbot.App.Services;

public class AdminCommandsService(CustomAudioPlayer audioPlayer)
{
    private VoiceNextConnection? _audioClient;
    private VoiceTransmitSink? _discordOutputStream;

    public async Task JoinVoiceChannelAsync(DiscordBotService discordBotService)
    {
        _audioClient = await GetGeneralVoiceChannel(discordBotService).ConnectAsync();
        _discordOutputStream = _audioClient.GetTransmitSink();
        audioPlayer.StreamToVoiceChannel(_discordOutputStream);
    }

    public async Task LeaveVoiceChannelAsync(DiscordBotService discordBotService)
    {
        // audioPlayer.UnStreamToVoiceChannel(_discordOutputStream!);
        _audioClient!.Disconnect();
        // _discordOutputStream = null;
        _audioClient = null;
    }

    private DiscordChannel GetGeneralVoiceChannel(DiscordBotService discordBotService) =>
        discordBotService
            .GetGuild()
            .Channels
            .SingleOrDefault(channel => channel.Value.Name == "General")
            .Value;

    public async Task ClearComputerAsync(DiscordBotService discordBotService)
    {
        var channel = discordBotService
            .GetGuild()
            .Channels
            .SingleOrDefault(channel => channel.Value.Name == "ordinateur")
            .Value;

        int count;
        do
        {
            var messages = await channel.GetMessagesAsync().ToListAsync();
            count = messages.Count;

            if (count > 0)
            {
                await channel.DeleteMessagesAsync(messages);
            }
        } while (count > 0);

    }
}
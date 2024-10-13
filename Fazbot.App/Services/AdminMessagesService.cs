using DSharpPlus.Entities;

namespace Fazbot.App.Services;

public class AdminMessagesService(AdminCommandsService adminCommandsService)
{
    public async Task ProcessAdminMessageAsync(DiscordBotService discordBotService, DiscordMessage message)
    {
        switch (message.Content)
        {
            case "!clearComputer":
                await adminCommandsService.ClearComputerAsync(discordBotService);
                break;
            case "!joinVoice":
                await adminCommandsService.JoinVoiceChannelAsync(discordBotService);
                break;
            case "!leaveVoice":
                await message.Channel!.SendMessageAsync("No workies!");
                //await adminCommandsService.LeaveVoiceChannelAsync(discordBotService);
                break;
            default:
                await message.Channel!.SendMessageAsync("Unknown command");
                break;
        }
    }
}
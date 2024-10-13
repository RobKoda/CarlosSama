using DSharpPlus.Entities;

namespace Fazbot.App.Services;

public class MessagesService(AdminMessagesService adminMessagesService, FazComputerMessagesService fazComputerMessagesService)
{
    public async Task ProcessMessageAsync(DiscordBotService discordBotService, DiscordMessage message)
    {
        switch (message.Channel!.Name)
        {
            case "bot-commands":
                await adminMessagesService.ProcessAdminMessageAsync(discordBotService, message);
                break;
            case "ordinateur":
                await fazComputerMessagesService.ProcessFazComputerMessageAsync(message);
                break;
        }
    }
}
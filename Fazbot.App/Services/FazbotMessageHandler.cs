using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Fazbot.App.Services;

public class FazbotMessageHandler(MessagesService messagesService)
{
    public async Task OnMessageCreated(DiscordClient sender, MessageCreatedEventArgs eventArgs, DiscordBotService discordBotService)
    {
        if (eventArgs.Author.IsBot) return;
        
        await messagesService.ProcessMessageAsync(discordBotService, eventArgs.Message);
    }
}
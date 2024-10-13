using DSharpPlus.Entities;

namespace Fazbot.App.Services;

public class FazComputerMessagesService(FazComputer fazComputer, AdminCommandsService adminCommandsService)
{
    public async Task ProcessFazComputerMessageAsync(DiscordMessage message)
    {
        if (fazComputer.IsLocked())
        {
            if (message.Content.Equals(FazComputer.Password, StringComparison.InvariantCultureIgnoreCase))
            {
                fazComputer.Unlock();
            }
            else
            {
                await message.Channel!.SendMessageAsync("Mot de passe incorrect !");
            }
            return;
        }
        
        switch (message.Content)
        {
            
            default:
                await message.Channel!.SendMessageAsync("Commande inconnue");
                break;
        }
    }

    public async Task Init(DiscordBotService discordBotService)
    {
        await adminCommandsService.ClearComputerAsync(discordBotService);
        var computerChannel = discordBotService.GetGuild().Channels.SingleOrDefault(channel => channel.Value.Name == "ordinateur").Value;
        await computerChannel.SendMessageAsync("""
                                               Bienvenue dans le Fazcomputer !
                                               V1.0
                                               Entrez le mot de passe :
                                               """);
    }
}
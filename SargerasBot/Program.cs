// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.WebSocket;
using SargerasBot.Commands;
using SargerasBot.Extensions;

namespace SargerasBot;

public static class Program {
    public static DiscordSocketClient Client;
    public static async Task Main() {
        Client = new DiscordSocketClient();
        Client.Log += message => message.Log();
        var token = Environment.GetEnvironmentVariable("SARGERAS_DISCORD_TOKEN");

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
        
        Client.Ready += Client_Ready;
        Client.SlashCommandExecuted += CommandHandler.SlashCommandHandler;

        await Task.Delay(-1);
    }
    
    public static async Task Client_Ready() {
        // await CommandHandler.RegisterCommands();
        await Sitrep.LoadFromDatabase();
    }
}
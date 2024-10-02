// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace SargerasBot;

public static class Program {
    public static DiscordSocketClient Client;
    public static async Task Main() {
        Client = new DiscordSocketClient();
        Client.Log += Log;
        var token = Environment.GetEnvironmentVariable("SARGERAS_DISCORD_TOKEN");

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
        
        Client.Ready += Client_Ready;
        Client.SlashCommandExecuted += SlashCommandHandler;

        await Task.Delay(-1);
    }
    
    public static async Task Client_Ready() {
        
    }
    
    private static async Task SlashCommandHandler(SocketSlashCommand command) {
        switch(command.Data.Name) {
            case "sitrep":
                await SitrepCommandHandler(command);
                break;
        }
    }

    private static async Task SitrepCommandHandler(SocketSlashCommand command) {
        var sender = command.User;
        var hours = command.Data.Options.First(x => x.Name.Equals("hours")).Value;
        
        await command.RespondAsync($"You executed {command.Data.Name} with {hours} hours.");
    }
    
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using SargerasBot.Extensions;

namespace SargerasBot.Commands;

public static class CommandHandler {
    
    public static async Task SlashCommandHandler(SocketSlashCommand command) {
        switch(command.Data.Name) {
            case "sitrep":
                await SitrepCommandHandler(command);
                break;
        }
    }

    private static async Task SitrepCommandHandler(SocketSlashCommand command) {
        var sender = command.User;
        // var hours = command.Data.Options.Where(x => x.Name.Equals("hours")).Value;
        
        switch (command.Data.Options.First().Name) {
            case "start":
                await command.RespondAsync($"Sitrep will now run every {DateTime.Now.DayOfWeek}");
                Sitrep.Start(command.Channel);
                break;
            case "stop":
                await command.RespondAsync($"Sitrep is now disabled");
                Sitrep.Stop();
                break;
            case "register":
                await command.RespondAsync($"You executed {command.Data.Name} with \"register\" parameter.");
                break;
        }
        
    }
    
    public static async Task RegisterCommands() {
        var sitrepCommand = new SlashCommandBuilder();
        sitrepCommand.WithName("sitrep");
        sitrepCommand.WithDescription("For developers to register the time they have spent on development for the given period");
        sitrepCommand.AddOption("start", ApplicationCommandOptionType.SubCommand, "Start the weekly sitrep");
        sitrepCommand.AddOption("stop", ApplicationCommandOptionType.SubCommand, "End the weekly sitrep");
        sitrepCommand.AddOption(new SlashCommandOptionBuilder().WithName("register").
            WithType(ApplicationCommandOptionType.SubCommand).WithDescription("Register your hours").
            AddOption("hours", ApplicationCommandOptionType.Integer, 
                "How many hours you have spent on the mod during the given period", isRequired: true));

        try {
            await Program.Client.CreateGlobalApplicationCommandAsync(sitrepCommand.Build());
        }
        catch(ApplicationCommandException exception) {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}
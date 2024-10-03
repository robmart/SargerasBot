using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

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
        var sender = command.User as SocketGuildUser;
        // var hours = command.Data.Options.Where(x => x.Name.Equals("hours")).Value;
        
        switch (command.Data.Options.First().Name) {
            case "start":
                if (sender.GuildPermissions.Administrator) {
                    if (Sitrep.Role == null) {
                        await command.RespondAsync(
                            $"You must set a role first. Please use the command `/sitrep role <role>`");
                    } else if (Sitrep.IsActive) {
                        await command.RespondAsync($"Sitrep is already active");
                    } else {
                        await command.RespondAsync($"Sitrep will now run every {DateTime.Now.DayOfWeek}");
                        Sitrep.Start(command.Channel);
                    }
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            case "stop":
                if (!Sitrep.IsActive) {
                    await command.RespondAsync($"Sitrep is already disabled");
                } else if (sender.GuildPermissions.Administrator) {
                    await command.RespondAsync($"Sitrep is now disabled");
                    Sitrep.Stop();
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            case "role":
                if (sender.GuildPermissions.Administrator) {
                    var role = command.Data.Options.First().Options.First().Value as IRole;
                    Sitrep.Role = role;
                    await command.RespondAsync($"{role.Name} is now the selected role for sitrep");
                    Sitrep.Stop();
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            case "register":
                if (Sitrep.Role != null && !sender.Roles.Contains(Sitrep.Role)) {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                } else if (Sitrep.Role == null) {
                    await command.RespondAsync($"Sitrep does not have a role assigned. To run the command, please have a server administrator run the command `/sitrep role <role>`");
                } else if (!Sitrep.IsActive) {
                    await command.RespondAsync(
                        $"Sitrep is currently disabled. To run the command, please have a server administrator run the command `/sitrep start`");
                } else {
                    var hours = (long) command.Data.Options.First().Options.First().Value;
                    await command.RespondAsync($"Registered {hours} hours for {sender.Username}");
                }
                break;
        }
        
    }
    
    public static async Task RegisterCommands() {
        var sitrepCommand = new SlashCommandBuilder();
        sitrepCommand.WithName("sitrep");
        sitrepCommand.WithDescription("For developers to register the time they have spent on development for the given period");
        sitrepCommand.WithContextTypes(InteractionContextType.Guild);
        
        sitrepCommand.AddOption("start", ApplicationCommandOptionType.SubCommand, "Start the weekly sitrep");
        
        sitrepCommand.AddOption("stop", ApplicationCommandOptionType.SubCommand, "End the weekly sitrep");
        
        sitrepCommand.AddOption(new SlashCommandOptionBuilder().WithName("role").
            WithType(ApplicationCommandOptionType.SubCommand).WithDescription("Set the role required to register sitrep").
            AddOption("role", ApplicationCommandOptionType.Role, 
                "Which role is required to register sitrep", isRequired: true));
        
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
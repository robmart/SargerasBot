using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using SargerasBot.Extensions;
using SargerasBot.Reference;
using SargerasBot.Sitrep;

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
        var channel = command.Channel as SocketGuildChannel;
        // var hours = command.Data.Options.Where(x => x.Name.Equals("hours")).Value;
        
        switch (command.Data.Options.First().Name) {
            case "start":
                if (sender.GuildPermissions.Administrator) {
                    if (Sitrep.Sitrep.Role == null) {
                        await command.RespondAsync(
                            $"You must set a role first. Please use the command `/sitrep role <role>`");
                    } else if (Sitrep.Sitrep.IsActive) {
                        await command.RespondAsync($"Sitrep is already active");
                    } else {
                        await command.RespondAsync($"Sitrep will now run every {DateTime.Now.DayOfWeek}");
                        Sitrep.Sitrep.Start(channel.Guild, command.Channel);
                        await Sitrep.Sitrep.Channel.Id.SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "SitrepChannel");
                    }
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            
            case "stop":
                if (!Sitrep.Sitrep.IsActive) {
                    await command.RespondAsync($"Sitrep is already disabled");
                } else if (sender.GuildPermissions.Administrator) {
                    await command.RespondAsync($"Sitrep is now disabled");
                    Sitrep.Sitrep.Stop();
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            
            case "role":
                if (sender.GuildPermissions.Administrator) {
                    var role = command.Data.Options.First().Options.First().Value as IRole;
                    Sitrep.Sitrep.Role = role;
                    await Sitrep.Sitrep.Role.Id.SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "SitrepRole");
                    await command.RespondAsync($"{role.Name} is now the selected role for sitrep");
                    Sitrep.Sitrep.Stop();
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            
            case "report":
                if (sender.GuildPermissions.Administrator) {
                    await command.RespondAsync($"Generating report, hold right");
                    await SitrepSheetBuilder.BuildSitrepSheet(channel.Guild.Id.ToString());
                    await command.Channel.SendFileAsync(Directory.GetCurrentDirectory() + "\\Sheet.xlsx");
                    File.Delete(Directory.GetCurrentDirectory() + "\\Sheet.xlsx");
                } else {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                }
                break;
            
            case "register":
                if (Sitrep.Sitrep.Role != null && !sender.Roles.Any(x => x.Id == Sitrep.Sitrep.Role.Id)) {
                    await command.RespondAsync($"You have insufficient permissions to run this command.");
                } else if (Sitrep.Sitrep.Role == null) {
                    await command.RespondAsync($"Sitrep does not have a role assigned. To run the command, please have a server administrator run the command `/sitrep role <role>`");
                } else if (!Sitrep.Sitrep.IsActive) {
                    await command.RespondAsync(
                        $"Sitrep is currently disabled. To run the command, please have a server administrator run the command `/sitrep start`");
                } else {
                    var options = command.Data.Options.First().Options;
                    var hours = (long)options.First(x => x.Name == "hours").Value;
                    var desc = (string)options.First(x => x.Name == "description").Value;
                    var progress = (string)(options.FirstOrDefault(x => x.Name == "progress") == null ? "" : options.FirstOrDefault(x => x.Name == "progress")?.Value);
                    var difficulties = (string)(options.FirstOrDefault(x => x.Name == "difficulties") == null ? "" : options.FirstOrDefault(x => x.Name == "difficulties")?.Value);
                    
                    await Sitrep.Sitrep.Register(sender, hours, desc, progress, difficulties);
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
        
        sitrepCommand.AddOption("report", ApplicationCommandOptionType.SubCommand, "Generate a report of the sitrep");
        
        sitrepCommand.AddOption(new SlashCommandOptionBuilder().WithName("role").
            WithType(ApplicationCommandOptionType.SubCommand).WithDescription("Set the role required to register sitrep").
            AddOption("role", ApplicationCommandOptionType.Role, 
                "Which role is required to register sitrep", isRequired: true));
        
        sitrepCommand.AddOption(new SlashCommandOptionBuilder().WithName("register").
            WithType(ApplicationCommandOptionType.SubCommand).WithDescription("Register your hours").
            AddOption("hours", ApplicationCommandOptionType.Integer, 
                "How many hours you have spent on the mod during the given period", isRequired: true).
            AddOption("description", ApplicationCommandOptionType.String, 
                "What you worked on during the period", isRequired: true).
            AddOption("progress", ApplicationCommandOptionType.String, 
                "What progress did you make during this period?", isRequired: false).
            AddOption("difficulties", ApplicationCommandOptionType.String, 
                "What difficulties did you encounter during this period?", isRequired: false));

        try {
            await Program.Client.CreateGlobalApplicationCommandAsync(sitrepCommand.Build());
        }
        catch(ApplicationCommandException exception) {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}
using Discord;

namespace SargerasBot.Extensions;

public static class LoggingExtensions {
    public static Task Log(this LogMessage msg) {
        return msg.ToString().Log();
    }
    
    public static Task Log(this string msg) {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
}
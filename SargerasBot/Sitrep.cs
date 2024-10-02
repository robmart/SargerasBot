using Discord.WebSocket;
using SargerasBot.Extensions;

namespace SargerasBot.Commands;

public static class Sitrep {
    
    public static bool IsActive { get; private set; }
    public static DateOnly StartDate { get; private set; }
    public static DateOnly EndDate { get; private set; }
    public static ISocketMessageChannel Channel { get; private set; }

    public static async void Start(ISocketMessageChannel channel) {
        IsActive = true;
        Channel = channel;

        if (StartDate.Year < 10 || EndDate.Year < 10) {
            await StartPeriod();
        }
    }

    internal static async void Start(ISocketMessageChannel channel, DateOnly startDate, DateOnly endDate) {
        StartDate = startDate;
        EndDate = endDate;
        
        Start(channel);

        if (DateOnly.FromDateTime(DateTime.Now) > EndDate) {
            await StartPeriod();
        }
    }

    public static void Stop() {
        IsActive = false;
        Channel = null;

        StartDate = DateOnly.MinValue;
        EndDate = DateOnly.MinValue;
    }

    private static async Task StartPeriod() {
        StartDate = DateOnly.FromDateTime(DateTime.Now);
        EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

        await Channel.SendMessageAsync($"New sitrep period `{StartDate}` - `{EndDate}`");
    }
}
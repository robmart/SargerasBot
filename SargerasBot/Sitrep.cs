using Discord.WebSocket;
using SargerasBot.Extensions;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SargerasBot.Commands;

public static class Sitrep {
    
    public static bool IsActive { get; private set; }
    public static DateOnly StartDate { get; private set; }
    public static DateOnly EndDate { get; private set; }
    public static ISocketMessageChannel Channel { get; private set; }
    public static Timer Timer { get; private set; }

    public static async void Start(ISocketMessageChannel channel) {
        if (IsActive) return;
        IsActive = true;
        Channel = channel;

        if (StartDate.Year < 10 || EndDate.Year < 10) {
            await StartPeriod();
        }
        
        var endDateTime = EndDate.ToDateTime(TimeOnly.MinValue);
        // var endDateTime = DateTime.Now.AddMinutes(2);
        Timer = new Timer(Convert.ToInt32(endDateTime.Subtract(DateTime.Now).TotalMilliseconds));
        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = false;
        Timer.Start();
    }

    internal static async void Start(ISocketMessageChannel channel, DateOnly startDate, DateOnly endDate) {
        if (IsActive) return;
        StartDate = startDate;
        EndDate = endDate;
        
        Start(channel);

        if (DateOnly.FromDateTime(DateTime.Now) > EndDate) {
            await StartPeriod();
        }
    }

    public static void Stop() {
        if (!IsActive) return;
        IsActive = false;
        Channel = null;

        StartDate = DateOnly.MinValue;
        EndDate = DateOnly.MinValue;
        Timer.Stop();
    }

    private static async Task StartPeriod() {
        StartDate = DateOnly.FromDateTime(DateTime.Now);
        EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

        await Channel.SendMessageAsync($"New sitrep period `{StartDate}` - `{EndDate}`");
    }
    
    private static async void TimerElapsed(object? sender, ElapsedEventArgs e) {
        await StartPeriod();
        
        var endDateTime = EndDate.ToDateTime(TimeOnly.MinValue);
        // var endDateTime = DateTime.Now.AddMinutes(2);
        Timer = new Timer(Convert.ToInt32(endDateTime.Subtract(DateTime.Now).TotalMilliseconds));
        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = false;
        Timer.Start();
    }
}
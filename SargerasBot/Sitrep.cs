using Discord.WebSocket;
using SargerasBot.Extensions;
using System.Timers;
using Discord;
using Timer = System.Timers.Timer;

namespace SargerasBot.Commands;

public static class Sitrep {
    
    /// <summary>
    /// Whether the sitrep function is active
    /// </summary>
    public static bool IsActive { get; private set; }
    /// <summary>
    /// The start of the sitrep period
    /// </summary>
    public static DateOnly StartDate { get; private set; }
    /// <summary>
    /// The end of the sitrep period
    /// </summary>
    public static DateOnly EndDate { get; private set; }
    /// <summary>
    /// When a new sitrep period will start
    /// </summary>
    public static DateOnly RefreshDate { get; private set; }
    /// <summary>
    /// The channel the bot will send the messages in
    /// </summary>
    public static ISocketMessageChannel Channel { get; private set; }
    /// <summary>
    /// The role needed to register sitrep
    /// </summary>
    public static IRole Role { get; internal set; }
    /// <summary>
    /// The timer for when the bot will start the next sitrep period
    /// </summary>
    public static Timer Timer { get; private set; }

    /// <summary>
    /// Starts the sitrep cycle with the default time period
    /// Start Date: One week in the past
    /// End Date: Today
    /// Refresh Date: One week in the future
    /// </summary>
    /// <param name="channel">The channel the bot will reply in</param>
    public static async void Start(ISocketMessageChannel channel) {
        if (IsActive) return;
        IsActive = true;
        Channel = channel;

        if (StartDate.Year < 10 || EndDate.Year < 10 || RefreshDate.Year < 10) {
            await StartPeriod();
        }
        
        var refreshDateTime = RefreshDate.ToDateTime(TimeOnly.MinValue);
        // var endDateTime = DateTime.Now.AddMinutes(2);
        Timer = new Timer(Convert.ToInt32(refreshDateTime.Subtract(DateTime.Now).TotalMilliseconds));
        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = false;
        Timer.Start();
    }

    /// <summary>
    /// Starts the sitrep cycle with a specified time period
    /// Refresh Date: One week in the future
    /// </summary>
    /// <param name="channel">The channel the bot will reply in</param>
    /// <param name="startDate">The start of the sitrep period</param>
    /// <param name="endDate">The end of the sitrep period</param>
    internal static async void Start(ISocketMessageChannel channel, DateOnly startDate, DateOnly endDate) {
        if (IsActive) return;
        StartDate = startDate;
        EndDate = endDate;
        RefreshDate = EndDate.AddDays(7);
        
        Start(channel);

        if (DateOnly.FromDateTime(DateTime.Now) > RefreshDate) {
            await StartPeriod();
        }
    }

    /// <summary>
    /// Stops the sitrep cycle
    /// </summary>
    public static void Stop() {
        if (!IsActive) return;
        IsActive = false;
        Channel = null;

        StartDate = DateOnly.MinValue;
        RefreshDate = DateOnly.MinValue;
        Timer.Stop();
    }

    /// <summary>
    /// Sets all the dates, relative to the current date
    /// </summary>
    private static async Task StartPeriod() {
        StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        EndDate = DateOnly.FromDateTime(DateTime.Now);
        RefreshDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

        await Channel.SendMessageAsync($"New sitrep period `{StartDate}` - `{EndDate}`");
    }
    
    /// <summary>
    /// Restarts the sitrep upon the timer expiring
    /// </summary>
    private static async void TimerElapsed(object? sender, ElapsedEventArgs e) {
        await StartPeriod();
        
        var refreshDateTime = RefreshDate.ToDateTime(TimeOnly.MinValue);
        // var endDateTime = DateTime.Now.AddMinutes(2);
        Timer = new Timer(Convert.ToInt32(refreshDateTime.Subtract(DateTime.Now).TotalMilliseconds));
        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = false;
        Timer.Start();
    }
}
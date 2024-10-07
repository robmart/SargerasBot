using System.Globalization;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Npgsql;
using SargerasBot.Extensions;
using SargerasBot.Reference;
using SargerasBot.Util;
using Timer = System.Timers.Timer;

namespace SargerasBot.Sitrep;

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
    /// The guild for this sitrep
    /// </summary>
    public static IGuild Guild { get; private set; }
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
    public static async void Start(IGuild guild, ISocketMessageChannel channel) {
        if (IsActive) return;
        IsActive = true;
        Guild = guild;
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
    internal static async void Start(IGuild guild, ISocketMessageChannel channel, DateOnly startDate, DateOnly endDate) {
        if (IsActive) return;
        StartDate = startDate;
        EndDate = endDate;
        RefreshDate = EndDate.AddDays(7);
        
        Start(guild, channel);

        if (DateOnly.FromDateTime(DateTime.Now) > RefreshDate) {
            await StartPeriod();
        }
    }

    /// <summary>
    /// Stops the sitrep cycle
    /// </summary>
    public static async Task Stop() {
        if (!IsActive) return;
        IsActive = false;
        Channel = null;

        StartDate = DateOnly.MinValue;
        RefreshDate = DateOnly.MinValue;
        Timer.Stop();
        
        await "NULL".SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "StartDate");
        await "NULL".SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "EndDate");

    }

    public static async Task Register(IUser user, long hours, string description, string progress = "", string difficulties = "") {
        await DatabaseUtil.AddSitrepData(DatabaseStrings.DatabaseSitrep, $"{Guild.Id.ToString()}", 
            $"{user.Username}", StartDate.ToString("O"), EndDate.ToString("O"), hours.ToString(), 
            description, progress, difficulties);
    }

    /// <summary>
    /// Sets all the dates, relative to the current date
    /// </summary>
    private static async Task StartPeriod() {
        StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        EndDate = DateOnly.FromDateTime(DateTime.Now);
        RefreshDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));
        
        await StartDate.ToString("O").SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "StartDate");
        await EndDate.ToString("O").SetServerData(DatabaseStrings.DatabaseSitrep, "ServerData", "EndDate");

        await Channel.SendMessageAsync($"{Role.Mention} New sitrep period `{StartDate}` - `{EndDate}`\n\nPlease register the amount of time you spent working on the mod during the period by using the command `/sitrep register <hours> <description>`\n\nIt would also be helpful if you could include:\n* The progress you have made (with the `<progress>` parameter)\n* Any difficulties you have encountered (with the `<difficulties>` parameter)");
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

    internal static async Task LoadFromDatabase() {
        await using var dataSource = NpgsqlDataSource.Create(DatabaseStrings.DatabaseSitrep);
        var exists = await dataSource.CreateCommand(
            "SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'serverdata';").ExecuteReaderAsync();
        if (exists.HasRows) {
            await using (var cmd = dataSource.CreateCommand("SELECT * FROM public.serverdata"))
            await using (var reader = await cmd.ExecuteReaderAsync()) {
                while (await reader.ReadAsync()) {
                    var guild = Program.Client.Guilds.First();
                    IChannel channel = null;
                    DateOnly startDate = DateOnly.MinValue;
                    DateOnly endDate = DateOnly.MinValue;
                    var channelResult = reader.GetValue(0);
                    if (channelResult != null && channelResult != DBNull.Value) {
                        var channelId = ulong.Parse((string)channelResult);
                        channel = guild.Channels.FirstOrDefault(x => x.Id == channelId);
                    }
                    var startDateResult = reader.GetValue(1);
                    if (startDateResult != null && startDateResult != DBNull.Value && !startDateResult.Equals("NULL")) {
                        startDate = DateOnly.Parse((string)startDateResult);
                    }
                    var endDateResult = reader.GetValue(2);
                    if (endDateResult != null && endDateResult != DBNull.Value && !endDateResult.Equals("NULL")) {
                        endDate = DateOnly.Parse((string)endDateResult);
                    }
                    var roleResult = reader.GetValue(3);
                    if (roleResult != null && roleResult != DBNull.Value) {
                        var roleId = ulong.Parse((string)roleResult);
                        Role = guild.Roles.FirstOrDefault(x => x.Id == roleId);
                    }

                    if (channel != null && startDate.Year > 10 && endDate.Year > 10) {
                        Start(guild, (ISocketMessageChannel)channel, startDate, endDate);
                    }
                }
            }
        }
    }
}
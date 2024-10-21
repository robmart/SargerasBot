using Npgsql;
using SargerasBot.Extensions;

namespace SargerasBot.Util;

public static class DatabaseUtil {
	public static async Task AddSitrepData(string connectionString, string guildId, string userName, string startDate, string endDate, string hours, string description, string progress, string difficulties) {
		await using var dataSource = NpgsqlDataSource.Create(connectionString);
		await dataSource.CreateCommand($"CREATE TABLE IF NOT EXISTS public.userdata (guildid TEXT, username TEXT, startdate TEXT, enddate TEXT, hours TEXT, description TEXT, progress TEXT, difficulties TEXT, unique(guildid, username, enddate));").ExecuteNonQueryAsync();
		await using (var cmd = dataSource.CreateCommand($@"
			            INSERT INTO userdata (guildid, username, startdate, enddate, hours, description, progress, difficulties)
			                VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
			                ON CONFLICT (guildid, username, enddate) DO UPDATE    
			                    SET startdate = EXCLUDED.startDate,   
									hours = EXCLUDED.hours,   
									description = EXCLUDED.description,   
									progress = EXCLUDED.progress,   
									difficulties = EXCLUDED.difficulties;")) {
			cmd.Parameters.AddWithValue(guildId); //Done this way because otherwise it inserts the values incorrectly
			cmd.Parameters.AddWithValue(userName);
			cmd.Parameters.AddWithValue(startDate);
			cmd.Parameters.AddWithValue(endDate);
			cmd.Parameters.AddWithValue(hours);
			cmd.Parameters.AddWithValue(description);
			cmd.Parameters.AddWithValue(progress);
			cmd.Parameters.AddWithValue(difficulties);
			await cmd.ExecuteNonQueryAsync();
		}
	}
}
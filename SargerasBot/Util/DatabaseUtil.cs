using Npgsql;
using SargerasBot.Extensions;

namespace SargerasBot.Util;

public static class DatabaseUtil {
	public static async Task AddSitrepData(string connectionString, string table, string startDate, string endDate, string hours) {
		await using var dataSource = NpgsqlDataSource.Create(connectionString);
		await dataSource.CreateCommand($"CREATE TABLE IF NOT EXISTS public.{table} (startdate TEXT, enddate TEXT, hours TEXT, unique(enddate));").ExecuteNonQueryAsync();
		await using (var cmd = dataSource.CreateCommand($@"
			            INSERT INTO {table} (startdate, enddate, hours)
			                VALUES ($1, $2, $3)
			                ON CONFLICT (enddate) DO UPDATE    
			                    SET startdate = EXCLUDED.startDate,   
									hours = EXCLUDED.hours;")) {
			cmd.Parameters.AddWithValue(startDate); //Done this way because otherwise it inserts the values incorrectly
			cmd.Parameters.AddWithValue(endDate);
			cmd.Parameters.AddWithValue(hours);
			await cmd.ExecuteNonQueryAsync();
		}
	}
}
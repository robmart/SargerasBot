using Npgsql;
using SargerasBot.Reference;

namespace SargerasBot.Extensions;

public static class DatabaseExtensions {
    
    public static async Task SetServerData(this object obj, string connectionString, string table, string column) {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await dataSource.CreateCommand($"CREATE TABLE IF NOT EXISTS public.{table} (sitrepchannel TEXT, startdate TEXT, enddate TEXT, sitreprole TEXT);").ExecuteNonQueryAsync();
        await dataSource.CreateCommand($"INSERT INTO {table} ({column}) SELECT 'foo' WHERE NOT EXISTS (SELECT * FROM {table})").ExecuteNonQueryAsync();
        await using (var cmd = dataSource.CreateCommand($"UPDATE {table} SET {column} = $1")) {
            cmd.Parameters.AddWithValue(obj.ToString());
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
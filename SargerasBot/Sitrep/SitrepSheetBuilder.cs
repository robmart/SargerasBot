using System.Text.RegularExpressions;
using Npgsql;
using OfficeOpenXml;
using SargerasBot.Reference;

namespace SargerasBot.Sitrep;

public static class SitrepSheetBuilder {
	public static async Task BuildSitrepSheet() {
		var data = await GetHours();
		using var package = new ExcelPackage(new FileInfo("Sitrep.xlsx"));
		foreach (var pair in data) {
			var workSheet = package.Workbook.Worksheets.Add(pair.Key);

			int row = 1;
			foreach (var dataValue in pair.Value) {
				workSheet.Cells[row, 1].Value = dataValue.Key;
				workSheet.Cells[row++, 2].Value = dataValue.Value;
			}
		}
			
		await package.SaveAsAsync(Directory.GetCurrentDirectory()+"\\Test.xlsx");
	}

	private static async Task<Dictionary<string, Dictionary<string, int>>> GetHours() {
		var data = new Dictionary<string, Dictionary<string, int>>();
		await using var dataSource = NpgsqlDataSource.Create(DatabaseStrings.DatabaseSitrep);
		await using (var reader = await dataSource.CreateCommand(
				             "SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND NOT table_name = 'serverdata';")
			             .ExecuteReaderAsync()) {
			while (await reader.ReadAsync()) {
				var table = reader.GetString(2);
				await using (var reader2 = await dataSource.CreateCommand($"SELECT * FROM {table}").ExecuteReaderAsync()) {
					while (await reader2.ReadAsync()) {
						var endDate = DateOnly.Parse(reader2.GetString(1));
						var month = Regex.Replace(endDate.ToString(), "...$", "");
						var hours = int.Parse(reader2.GetString(2));
						
						if (!data.ContainsKey(month)) {
							data.Add(month, new Dictionary<string, int>());
						}

						if (!data[month].ContainsKey(table)) {
							data[month].Add(table, hours);
						} else {
							data[month][table] = hours + data[month][table];
						}
					}
				}
			}
		}
		
		return data;
	}
}
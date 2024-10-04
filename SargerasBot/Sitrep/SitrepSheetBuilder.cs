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
				workSheet.Cells[row, 1].Value = dataValue.Name;
				workSheet.Cells[row, 2].Value = dataValue.Hours;
				workSheet.Cells[row, 3].Value = dataValue.Description;
				workSheet.Cells[row, 4].Value = dataValue.Progress;
				workSheet.Cells[row++, 5].Value = dataValue.Difficulties;
			}
		}
			
		await package.SaveAsAsync(Directory.GetCurrentDirectory()+"\\Sheet.xlsx");
	}

	private static async Task<Dictionary<string, List<UserData>>> GetHours() {
		var data = new Dictionary<string, List<UserData>>();
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
						var description = reader2.GetString(3);
						var progress = reader2.GetString(4);
						var difficulties = reader2.GetString(5);
						
						if (!data.ContainsKey(month)) {
							data.Add(month, new List<UserData>());
						}

						if (!data[month].Any(x => x.Name.Equals(table))) {
							data[month].Add(new UserData(table, hours, description, progress, difficulties));
						} else {
							data[month].First(x => x.Name.Equals(table)).Hours += hours;
						}
					}
				}
			}
		}
		
		return data;
	}
}
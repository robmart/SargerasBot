using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;
using OfficeOpenXml;
using SargerasBot.Extensions;
using SargerasBot.Reference;

namespace SargerasBot.Sitrep;

public static class SitrepSheetBuilder {
	public static async Task BuildSitrepSheet(string guildId) {
		var data = await GetHours(guildId);
		using var package = new ExcelPackage(new FileInfo("Sitrep.xlsx"));
		foreach (var pair in data) {
			var workSheet = package.Workbook.Worksheets.Add(pair.Key);
			int row = 1;

			foreach (var dataValue in pair.Value) {
				foreach (var dataValueInstance in dataValue.Instances) {
					workSheet.Cells[row, 1].Value = dataValue.Name;
					workSheet.Cells[row, 2].Value = dataValueInstance.Hours;
					workSheet.Cells[row, 3].Value = dataValueInstance.Description;
					workSheet.Cells[row, 4].Value = dataValueInstance.Progress;
					workSheet.Cells[row++, 5].Value = dataValueInstance.Difficulties;
				}
			}

			row++;

			foreach (var dataValue in pair.Value) {
				workSheet.Cells[row, 1].Value = dataValue.Name;
				workSheet.Cells[row++, 2].Value = dataValue.TotalHours;
			}
		}
			
		await package.SaveAsAsync(Directory.GetCurrentDirectory()+"\\Sheet.xlsx");
	}

	private static async Task<Dictionary<string, List<UserData>>> GetHours(string guildId) {
		var data = new Dictionary<string, List<UserData>>();
		await using var dataSource = NpgsqlDataSource.Create(DatabaseStrings.DatabaseSitrep);
		await using (var cmd = dataSource.CreateCommand($"SELECT * FROM userdata WHERE guildid = $1")) {
			cmd.Parameters.AddWithValue(guildId);
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync()) {
					var userName = reader.GetString(1);
					var endDate = DateOnly.Parse(reader.GetString(3));
					var month = Regex.Replace(endDate.ToString("O"), "...$", "");
					var hours = int.Parse(reader.GetString(4));
					var description = reader.GetString(5);
					var progress = reader.GetString(6);
					var difficulties = reader.GetString(7);
					
					if (!data.ContainsKey(month)) {
						data.Add(month, new List<UserData>());
					}

					if (!data[month].Any(x => x.Name.Equals(userName))) {
						data[month].Add(new UserData(userName));
					}
					
					data[month].First(x => x.Name.Equals(userName)).Instances.Add(new UserData.UserDataInstance(hours, description, progress, difficulties));
				}
		}
		
		return data;
	}
}
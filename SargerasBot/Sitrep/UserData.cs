namespace SargerasBot.Sitrep;

public class UserData {
	public string Name { get; set; }
	public int Hours { get; set; }
	public string Description { get; set; }
	public string Progress { get; set; }
	public string Difficulties { get; set; }

	public UserData(string name, int hours, string description, string progress, string difficulties) {
		Name = name;
		Hours = hours;
		Description = description;
		Progress = progress;
		Difficulties = difficulties;
	}
}
namespace SargerasBot.Sitrep;

public class UserData {
	public string Name { get; set; }
	public int TotalHours => Instances.Sum(userDataInstance => userDataInstance.Hours);
	public List<UserDataInstance> Instances { get; set; } =  new List<UserDataInstance>();

	public UserData(string name) {
		Name = name;
	}

	public class UserDataInstance {
		public int Hours { get; set; }
		public string Description { get; set; }
		public string Progress { get; set; }
		public string Difficulties { get; set; }

		public UserDataInstance(int hours, string description, string progress, string difficulties) {
			Hours = hours;
			Description = description;
			Progress = progress;
			Difficulties = difficulties;
		}
	}
}
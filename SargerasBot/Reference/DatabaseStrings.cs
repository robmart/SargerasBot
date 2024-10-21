namespace SargerasBot.Reference;

public static class DatabaseStrings {
    public static string DatabaseBase => 
        $"Host={Environment.GetEnvironmentVariable("SARGERAS_DATABASE_IP")};" +
        $"Port={Environment.GetEnvironmentVariable("SARGERAS_DATABASE_PORT")};" +
        $"Username={Environment.GetEnvironmentVariable("SARGERAS_DATABASE_USERNAME")};" +
        $"Password={Environment.GetEnvironmentVariable("SARGERAS_DATABASE_PASSWORD")};";

    public static string DatabaseSitrep => $"{DatabaseBase}Database=Sitrep";
}
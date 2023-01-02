namespace Develop.API.Configuration;

public sealed record SqlDbSettings
{
	public string Host { get; set; } = null!;
	public int Port { get; set; }
	public string User { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string Catalog { get; set; } = null!;

	public string ConnectionString() => $"Host={Host};Port={Port};Database={Catalog};Username={User};Password={Password}";

	public override string ToString() => $"{User}@{Host}:{Port}/{Catalog}";
}
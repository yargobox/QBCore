namespace Example1.DAL.Configuration;

public sealed record MongoDbSettings
{
	public string Host { get; set; } = null!;
	public int Port { get; set; }
	public string User { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string Catalog { get; set; } = null!;

	public string ConnectionString => $"mongodb://{User}:{Password}@{Host}:{Port}";

	public override string ToString() => $"{User}@{Host}:{Port}/{Catalog}";
}
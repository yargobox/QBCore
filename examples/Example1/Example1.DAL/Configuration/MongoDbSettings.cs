namespace Example1.DAL.Configuration;

public sealed record MongoDbSettings
{
	public string Host { get; set; } = null!;
	public int Port { get; set; }
	public string User { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string Catalog { get; set; } = null!;

	public override string ToString() => $"mongodb://{User}:{Password}@{Host}:{Port}";
}
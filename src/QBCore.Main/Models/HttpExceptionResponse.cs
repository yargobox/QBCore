using System.Text.Json;

namespace QBCore.Controllers.Models;

public record HttpExceptionResponse
{
	public string? Instance { get; set; }
	public string? Message { get; set; }
	public string? Details { get; set; }
	public string? Type { get; set; }
	public int StatusCode { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
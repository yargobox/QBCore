using System.Text.Json;

namespace Example1.API.Models;

public record HttpExceptionResponse
{
	public int StatusCode { get; init; }
	public string? Instance { get; init; }
	public string? Message { get; init; }
	public string? Details { get; init; }
	public string? Type { get; init; }
	public string? StackTrace { get; init; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
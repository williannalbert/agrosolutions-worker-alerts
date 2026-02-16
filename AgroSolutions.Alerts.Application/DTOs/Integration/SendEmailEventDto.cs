namespace AgroSolutions.Alerts.Application.DTOs.Integration;

public record SendEmailEventDto
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public string? TextBody { get; init; }
    public string? From { get; init; }
    public List<string> Cc { get; init; } = new();
    public List<string> Bcc { get; init; } = new();
    public Dictionary<string, string> Metadata { get; init; } = new();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
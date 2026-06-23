namespace IoTPlatform.Models.Entities.Logging;

/// <summary>
/// Structured application log sink (written by Serilog). Append-only.
/// </summary>
public class SystemLog
{
    public long Id { get; set; }
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? Properties { get; set; } // jsonb
    public Guid? CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

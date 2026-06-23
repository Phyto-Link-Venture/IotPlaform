namespace IoTPlatform.Common.Results;

/// <summary>
/// Describes a failure with a machine-readable code and a human-readable message.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error NotFound(string message, string code = "NotFound") => new(code, message, ErrorType.NotFound);
    public static Error Validation(string message, string code = "Validation") => new(code, message, ErrorType.Validation);
    public static Error Conflict(string message, string code = "Conflict") => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string message, string code = "Unauthorized") => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string message, string code = "Forbidden") => new(code, message, ErrorType.Forbidden);
    public static Error Failure(string message, string code = "Failure") => new(code, message, ErrorType.Failure);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}

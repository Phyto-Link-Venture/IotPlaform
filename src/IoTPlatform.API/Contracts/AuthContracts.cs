namespace IoTPlatform.API.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid UserId, string Email);

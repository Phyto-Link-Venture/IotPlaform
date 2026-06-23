namespace IoTPlatform.Common.Abstractions;

/// <summary>
/// Encrypts/decrypts sensitive values (e.g. AI provider API keys) at rest.
/// Default implementation uses ASP.NET Core Data Protection; can be swapped for a
/// KMS / secret manager without touching callers or the schema.
/// </summary>
public interface IKeyProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}

using IoTPlatform.Common.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace IoTPlatform.Infrastructure.Security;

/// <summary>
/// <see cref="IKeyProtector"/> backed by ASP.NET Core Data Protection. The key ring
/// is persisted to a mounted volume (configured at startup) so secrets survive restarts.
/// </summary>
public sealed class DataProtectionKeyProtector : IKeyProtector
{
    private const string Purpose = "IoTPlatform.AiProviderApiKey";
    private readonly IDataProtector _protector;

    public DataProtectionKeyProtector(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector(Purpose);

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}

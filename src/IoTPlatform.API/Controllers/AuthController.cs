using IoTPlatform.API.Authentication;
using IoTPlatform.API.Contracts;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Models.Entities.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    AppDbContext db,
    JwtTokenService tokenService,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    /// <summary>Authenticates a user by email + password and returns a JWT access token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        // Login must look across tenants, so bypass the company query filter for the lookup.
        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive && !u.IsDeleted, cancellationToken);

        if (user is null)
            return Unauthorized();

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = tokenService.CreateAccessToken(user);
        return new LoginResponse(token, expiresAt, user.Id, user.Email);
    }
}

using System.Security.Cryptography;
using System.Text;
using IoTPlatform.API.Authentication;
using IoTPlatform.API.Authorization;
using IoTPlatform.API.Mcp;
using IoTPlatform.Common.Abstractions;
using IoTPlatform.Infrastructure;
using IoTPlatform.Models.Entities.Tenancy;
using IoTPlatform.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging (console now; DB sink configurable via appsettings).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// --- Options ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// --- Request-scoped context (current user + tenant) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// --- Layers ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// --- Auth ---
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<JwtTokenService>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

// The signing key must come from configuration (env var / secret). In Development we
// generate an ephemeral key so the app runs without configuration; outside Development a
// missing key is a hard failure. No secret is ever hardcoded here.
if (string.IsNullOrWhiteSpace(jwt.SigningKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "Jwt:SigningKey is not configured. Set it via the Jwt__SigningKey environment variable / secret.");

    jwt.SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
}

// Ensure JwtTokenService (via IOptions) sees the resolved key.
builder.Services.Configure<JwtOptions>(o => o.SigningKey = jwt.SigningKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// --- CORS (frontend) ---
const string FrontendCors = "frontend";
builder.Services.AddCors(options => options.AddPolicy(FrontendCors, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// --- API + Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- MCP server (external LLM integration). Tools reuse the platform services. ---
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PlatformMcpTools>();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCors);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp");

app.Run();

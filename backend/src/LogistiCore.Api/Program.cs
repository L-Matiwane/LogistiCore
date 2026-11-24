using System.Security.Claims;
using System.Text;
using LogistiCore.Api.Auth;
using LogistiCore.Api.Endpoints;
using LogistiCore.Domain.Entities;
using LogistiCore.Infrastructure;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing. Set it with dotnet user-secrets.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenService.Issuer,
            ValidateAudience = true,
            ValidAudience = TokenService.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// Allow the React dev site to call this API
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DevDataSeeder.SeedPasswordsAsync(db);
}

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { database = "connected" })
        : Results.Problem("Cannot reach the database");
});

app.MapPost("/api/v1/auth/login", async (
    LoginRequest request,
    AppDbContext db,
    IPasswordHasher<User> hasher,
    TokenService tokens) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    // Same answer for "no such user" and "wrong password" so attackers learn nothing.
    if (user is null ||
        hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        token = tokens.CreateToken(user),
        fullName = user.FullName,
        role = user.Role.Name
    });
});

app.MapGet("/api/v1/auth/me", (ClaimsPrincipal principal) => Results.Ok(new
{
    name = principal.FindFirstValue(ClaimTypes.Name),
    email = principal.FindFirstValue(ClaimTypes.Email),
    role = principal.FindFirstValue(ClaimTypes.Role)
})).RequireAuthorization();

app.MapGet("/api/v1/admin/ping", () => Results.Ok(new { message = "Hello, Admin." }))
    .RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapParcelEndpoints();
app.MapWarehouseEndpoints();
app.MapDispatchEndpoints();

app.Run();

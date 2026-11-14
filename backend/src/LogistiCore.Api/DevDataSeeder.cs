using LogistiCore.Domain.Entities;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Api.Auth;

public static class DevDataSeeder
{
    // Demo-only password for the seeded test accounts. Never use it for anything real.
    public const string DemoPassword = "LogistiCore#2026";

    public static async Task SeedPasswordsAsync(AppDbContext db)
    {
        var hasher = new PasswordHasher<User>();
        var users = await db.Users.Where(u => u.PasswordHash == "PLACEHOLDER").ToListAsync();

        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, DemoPassword);
        }

        if (users.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}
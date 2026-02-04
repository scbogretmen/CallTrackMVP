using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CallTrackMVP.Web.Models;

namespace CallTrackMVP.Web.Data;

public static class DbInitializer
{
    public static async Task EnsureCallLogUpdatesTableExistsAsync(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='CallLogUpdates'";
            var exists = await cmd.ExecuteScalarAsync();
            if (exists == null)
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS CallLogUpdates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CallLogId INTEGER NOT NULL,
    GuncellenenTarih TEXT NOT NULL,
    GuncellenenCagriSaat TEXT NOT NULL,
    GuncellemeNedeni TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (CallLogId) REFERENCES CallLogs(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_CallLogUpdates_CallLogId ON CallLogUpdates(CallLogId);
";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally { conn.Close(); }
    }

    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.AppUsers.AnyAsync())
            return;

        var hasher = new PasswordHasher<AppUser>();

        var users = new[]
        {
            new AppUser { UserName = "admin", FullName = "Yönetici", Role = "Admin" },
            new AppUser { UserName = "ahmet", FullName = "Ahmet", Role = "User" },
            new AppUser { UserName = "ayse", FullName = "Ayşe", Role = "User" },
            new AppUser { UserName = "kubra", FullName = "Kübra", Role = "User" },
            new AppUser { UserName = "mehmet", FullName = "Mehmet", Role = "User" }
        };

        var passwords = new[] { "Admin123!", "Ahmet123!", "Ayse123!", "Kubra123!", "Mehmet123!" };

        for (int i = 0; i < users.Length; i++)
        {
            users[i].PasswordHash = hasher.HashPassword(users[i], passwords[i]);
            db.AppUsers.Add(users[i]);
        }

        await db.SaveChangesAsync();
    }

    public static async Task EnsureCallTypesAsync(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='CallTypes'";
            if (await cmd.ExecuteScalarAsync() == null)
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS CallTypes (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS IX_CallTypes_Name ON CallTypes(Name);
";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally { conn.Close(); }

        if (await db.CallTypes.AnyAsync())
            return;
        var types = new[] { "Web", "Telefon", "Eposta" };
        foreach (var name in types)
            db.CallTypes.Add(new CallType { Name = name });
        await db.SaveChangesAsync();
    }

    public static async Task EnsureCallLogAcknowledgmentsTableExistsAsync(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='CallLogAcknowledgments'";
            if (await cmd.ExecuteScalarAsync() == null)
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS CallLogAcknowledgments (
    UserId INTEGER NOT NULL,
    CallLogId INTEGER NOT NULL,
    LastAcknowledgedUpdateId INTEGER NOT NULL,
    PRIMARY KEY (UserId, CallLogId),
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (CallLogId) REFERENCES CallLogs(Id) ON DELETE CASCADE
);
";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally { conn.Close(); }
    }
}

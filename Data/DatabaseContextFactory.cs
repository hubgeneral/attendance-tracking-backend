using attendance_tracking_backend;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace attendance_tracking_backend.Data
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {

        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            // ✅ match the same provider + connection string you use in Program.cs
            optionsBuilder.UseNpgsql("Host=localhost;Database=attendance_tracking_db;Username=postgres;Password=lakers");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
/*
Why this works

At runtime, your app builds the DbContext through DI in Program.cs.

At design-time (dotnet ef migrations add / update), EF Core doesn’t have your DI container, so it uses this IDesignTimeDbContextFactory instead.

That factory tells EF how to construct the context manually.*/
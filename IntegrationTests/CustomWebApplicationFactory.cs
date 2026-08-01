using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repository.Data;
using Domain.Models;
using Domain.Enums;
using BCrypt.Net;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.MsSql;

namespace HealthcarePortal.IntegrationTests;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
    .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(x =>
        {
            x.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            x.AddDbContext<AppDbContext>( options =>
                options.UseSqlServer(_dbContainer.GetConnectionString())
                .EnableSensitiveDataLogging()
            );

            // Seed the database with an admin user
            using (var scope = x.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                SeedAdminUser(db);
            }
        });
    }

    private static void SeedAdminUser(AppDbContext db)
    {
        if (db.Users.Any(u => u.Email == "admin@test.com"))
            return;

        var adminUser = new User
        {
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        db.Users.Add(adminUser);
        db.SaveChanges();
    }

    public async Task InitializeAsync()
    {

       await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}

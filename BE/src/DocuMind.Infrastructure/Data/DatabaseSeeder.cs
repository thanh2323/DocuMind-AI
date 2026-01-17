using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocuMind.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<SqlServer>>();

            try
            {
                var context = serviceProvider.GetRequiredService<SqlServer>();
                var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

                // Ensure database is created
                await context.Database.MigrateAsync();

                // Manually install Hangfire tables using a FRESH connection
                // This prevents closing the shared DbContext connection
                var connectionString = context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        Hangfire.SqlServer.SqlServerObjectsInstaller.Install(connection);
                    }
                }

                // Seed Admin User
                if (!await context.Users.AnyAsync(u => u.Role == "Admin"))
                {
                    logger.LogInformation("Creating Admin user...");
                    var adminUser = new User
                    {
                        FullName = "System Admin",
                        Email = "admin@gmail.com",
                        Role = "Admin",
                        PasswordHash = passwordHasher.HashPassword("123123"), // Default password
                        IsLocked = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Admin user created successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                // Ensure the app doesn't crash on seed failure, but log it critical
            }
        }
    }
}

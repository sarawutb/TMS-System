using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Security;

namespace TmsSystem.Infrastructure.Data;

public sealed class TmsDbInitializer(TmsDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await TableExistsAsync("sec_role", cancellationToken) || !await TableExistsAsync("sec_user", cancellationToken))
        {
            return;
        }

        if (!await dbContext.Roles.AnyAsync(cancellationToken))
        {
            dbContext.Roles.AddRange(
                new Role { RoleCode = "Administrator", RoleName = "Administrator", Permissions = "ALL", CreateBy = "seed" },
                new Role { RoleCode = "Planner", RoleName = "Planner", CreateBy = "seed" },
                new Role { RoleCode = "Dispatcher", RoleName = "Dispatcher", CreateBy = "seed" },
                new Role { RoleCode = "ControlTower", RoleName = "Control Tower", CreateBy = "seed" },
                new Role { RoleCode = "Fleet", RoleName = "Fleet", CreateBy = "seed" },
                new Role { RoleCode = "Finance", RoleName = "Finance", CreateBy = "seed" });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var adminRole = await dbContext.Roles.FirstAsync(x => x.RoleCode == "Administrator", cancellationToken);
            dbContext.Users.Add(new User
            {
                Username = "admin",
                FullName = "System Administrator",
                Email = "admin@tms.local",
                RoleId = adminRole.RoleId,
                PasswordHash = PasswordHasher.HashPassword("Admin@12345"),
                CreateBy = "seed"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CASE WHEN OBJECT_ID(@tableName, 'U') IS NULL THEN 0 ELSE 1 END";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = $"dbo.{tableName}";
        command.Parameters.Add(parameter);

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }
}

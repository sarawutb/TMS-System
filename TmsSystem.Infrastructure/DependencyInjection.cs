using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TmsSystem.Application.Interfaces;
using TmsSystem.Application.Interfaces.Planning;
using TmsSystem.Infrastructure.Data;
using TmsSystem.Infrastructure.Security;
using TmsSystem.Infrastructure.Services;
using TmsSystem.Infrastructure.Services.Planning;

namespace TmsSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TmsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<TmsDbInitializer>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IOptimizationSolverService, HeuristicOptimizationSolverService>();
        services.AddScoped<IPlanningService, PlanningService>();
        return services;
    }
}

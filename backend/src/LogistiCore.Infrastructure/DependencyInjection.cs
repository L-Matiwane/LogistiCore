using LogistiCore.Application.Customers;
using LogistiCore.Application.Dashboard;
using LogistiCore.Application.Parcels;
using LogistiCore.Infrastructure.Customers;
using LogistiCore.Infrastructure.Dashboard;
using LogistiCore.Infrastructure.Parcels;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogistiCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default")));

        services.AddScoped<IParcelService, ParcelService>();
        services.AddScoped<ICustomerLookup, CustomerLookup>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}

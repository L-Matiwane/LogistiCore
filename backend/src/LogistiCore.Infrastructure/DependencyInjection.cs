using LogistiCore.Application.Customers;
using LogistiCore.Application.Dashboard;
using LogistiCore.Application.Dispatch;
using LogistiCore.Application.Parcels;
using LogistiCore.Application.Warehouse;
using LogistiCore.Infrastructure.Customers;
using LogistiCore.Infrastructure.Dashboard;
using LogistiCore.Infrastructure.Dispatch;
using LogistiCore.Infrastructure.Parcels;
using LogistiCore.Infrastructure.Persistence;
using LogistiCore.Infrastructure.Warehouse;
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
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IDispatchService, DispatchService>();
        return services;
    }
}

using Application.Deals;
using Application.Messages;
using Application.Modules;
using Application.Requirements;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDealService, DealService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IRequirementService, RequirementService>();
        services.AddScoped<ModuleService, ModuleService>();
        return services;
    }
}

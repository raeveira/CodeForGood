using Application.Deals;
using Application.Messages;
using Application.Requirements;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDealCollection, DealCollection>();
        services.AddScoped<IMessageCollection, MessageCollection>();
        services.AddScoped<IRequirementCollection, RequirementCollection>();
        return services;
    }
}

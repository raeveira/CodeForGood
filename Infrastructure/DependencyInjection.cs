using Application;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDealRepository, MockDealRepository>();
        services.AddSingleton<IMessageRepository, MockMessageRepository>();
        services.AddSingleton<IRequirementRepository, MockRequirementRepository>();
        return services;
    }
}

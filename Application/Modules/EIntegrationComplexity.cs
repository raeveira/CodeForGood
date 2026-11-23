namespace Application.Modules;

public enum EIntegrationComplexity
{
    PlugAndPlay = 1,
    ConfigurationNeeded = 2,
    CustomIntegration = 3
}

public static class IntegrationComplexityExtensions
{
    public static string ToLabel(this EIntegrationComplexity complexity) => complexity switch
    {
        EIntegrationComplexity.PlugAndPlay => "Plug and play",
        EIntegrationComplexity.ConfigurationNeeded => "Configuration needed",
        EIntegrationComplexity.CustomIntegration => "Custom integration",
        _ => throw new ArgumentOutOfRangeException(nameof(complexity))
    };
}

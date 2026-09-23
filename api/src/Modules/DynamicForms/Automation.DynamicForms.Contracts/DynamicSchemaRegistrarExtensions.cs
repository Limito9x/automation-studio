using Microsoft.Extensions.DependencyInjection;

namespace Automation.DynamicForms.Contracts;

public class RegisteredDynamicSchema
{
    public string OwnerType { get; }

    public RegisteredDynamicSchema(string ownerType)
    {
        OwnerType = ownerType;
    }
}

public static class DynamicSchemaRegistrarExtensions
{
    public static IServiceCollection AddDynamicSchema(
        this IServiceCollection services, 
        string ownerType)
    {
        services.AddSingleton(new RegisteredDynamicSchema(ownerType));
        return services;
    }
}


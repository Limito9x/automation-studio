using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Automation.SharedKernel.Abstractions.Caching;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Automation.SharedKernel.Infrastructure.Caching;

public class CachingPolicy : IHandlerPolicy
{
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, JasperFx.IServiceContainer container)
    {
        foreach (var chain in chains)
        {
            // Check if the message type implements ICachedQuery<TResponse>
            var cachedQueryInterface = chain.MessageType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICachedQuery<>));

            if (cachedQueryInterface != null)
            {
                // Register FusionCacheMiddleware. Wolverine will automatically close the generic 
                // BeforeAsync and AfterAsync methods using the concrete Message type and Response type!
                chain.Middleware.Add(new MethodCall(typeof(FusionCacheMiddleware), nameof(FusionCacheMiddleware.BeforeAsync)));
                chain.Middleware.Add(new MethodCall(typeof(FusionCacheMiddleware), nameof(FusionCacheMiddleware.AfterAsync)));
            }
        }
    }
}




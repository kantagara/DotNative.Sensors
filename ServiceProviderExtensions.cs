using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.Sensors;

public static class SensorsServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public ISensors Sensors => services.GetRequiredService<ISensors>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static ISensors Sensors(this IServiceProvider services) =>
        services.GetRequiredService<ISensors>();
#endif
}

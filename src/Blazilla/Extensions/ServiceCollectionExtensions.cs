using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blazilla.Extensions;

/// <summary>
/// Provides extension methods for registering FluentValidation validators with <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all validators from the specified assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the validators to.</param>
    /// <param name="assemblies">The assemblies to scan for validators.</param>
    /// <param name="lifetime">The service lifetime for the registered validators. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddValidatorsFromAssemblies(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        foreach (var assembly in assemblies)
            services.AddValidatorsFromAssembly(assembly, lifetime);

        return services;
    }

    /// <summary>
    /// Registers all validators from the specified assembly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the validators to.</param>
    /// <param name="assembly">The assembly to scan for validators.</param>
    /// <param name="lifetime">The service lifetime for the registered validators. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        AssemblyScanner
            .FindValidatorsInAssembly(assembly)
            .ForEach(scanResult => services.AddScanResult(scanResult, lifetime));

        return services;
    }

    /// <summary>
    /// Registers all validators from the assembly containing the specified type.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the validators to.</param>
    /// <param name="type">The type whose assembly will be scanned for validators.</param>
    /// <param name="lifetime">The service lifetime for the registered validators. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddValidatorsFromAssemblyContaining(
        this IServiceCollection services,
        Type type,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        => services.AddValidatorsFromAssembly(type.Assembly, lifetime);

    /// <summary>
    /// Registers all validators from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly will be scanned for validators.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the validators to.</param>
    /// <param name="lifetime">The service lifetime for the registered validators. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddValidatorsFromAssemblyContaining<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        => services.AddValidatorsFromAssembly(typeof(T).Assembly, lifetime);

    private static IServiceCollection AddScanResult(this IServiceCollection services, AssemblyScanner.AssemblyScanResult scanResult, ServiceLifetime lifetime)
    {
        //Register as interface
        services.TryAddEnumerable(
            new ServiceDescriptor(
                serviceType: scanResult.InterfaceType,
                implementationType: scanResult.ValidatorType,
                lifetime: lifetime));

        //Register as self
        services.TryAdd(
            new ServiceDescriptor(
                serviceType: scanResult.ValidatorType,
                implementationType: scanResult.ValidatorType,
                lifetime: lifetime));

        return services;
    }
}

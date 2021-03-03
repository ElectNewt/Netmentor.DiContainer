using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Netmentor.DiContainer
{
    public static class DiModuleUtils
    {
        /// <summary>
        /// Apply the services in this module to another IServiceCollection
        /// Will only be applied a single time to each collection, otherwise does nothing 
        /// </summary>
        public static IServiceCollection ApplyModule(this IServiceCollection services, DiModule module)
        {
            return module.Apply(services);
        }

        /// <summary>
        /// Apply the services in this module to another IServiceCollection
        /// Will only be applied a single time to each collection, otherwise does nothing 
        /// </summary>
        public static DiModule ApplyModule(this DiModule services, DiModule module)
        {
            return (DiModule)ApplyModule((IServiceCollection)services, module);
        }

        /// <summary>
        /// Adds the TImplemenation to DI and then adds the TAbstract, ensuring that
        /// the same object is returned within the scope 
        /// </summary>
        public static DiModule AddScoped<TAbstract, TImplementation>(this DiModule module, Func<IServiceProvider, TImplementation>? factory = null)
            where TAbstract : class
            where TImplementation : class, TAbstract
        {
            module = factory == null
                ? module.AddScoped<TImplementation>()
                : module.AddScoped(factory);

            return module
                .AddScoped(x => x.GetService<TImplementation>() as TAbstract ?? throw new Exception("something is not working well"));
        }

        public static DiModule AddScoped<T>(this DiModule module, Func<IServiceProvider, T>? factory = null)
            where T : class
        {
            module = factory == null
                ? module.AddDependencies<T>()
                : module;

            return (DiModule)(factory == null
                ? ((IServiceCollection)module).AddScoped<T>()
                : ((IServiceCollection)module).AddScoped(factory));
        }

        /// <summary>
        /// Adds the TImplemenation to DI and then adds the TAbstract, ensuring that
        /// the same object is returned within the scope 
        /// </summary>
        public static DiModule AddSingleton<TAbstract, TImplementation>(this DiModule module, Func<IServiceProvider, TImplementation>? factory = null)
            where TAbstract : class
            where TImplementation : class, TAbstract
        {
            module = factory == null
                ? module.AddDependencies<TImplementation>()
                : module;

            module = factory == null
                ? module.AddSingleton<TImplementation>()
                : module.AddSingleton(factory);

            return module
                .AddSingleton(x => x.GetService<TImplementation>() as TAbstract ?? throw new Exception("something is not working well"));
        }

        public static DiModule AddSingleton<T>(this DiModule module, Func<IServiceProvider, T>? factory = null)
            where T : class
        {
            module = factory == null
                ? module.AddDependencies<T>()
                : module;

            return (DiModule)(factory == null
                ? ((IServiceCollection)module).AddSingleton<T>()
                : ((IServiceCollection)module).AddSingleton(factory));
        }

        /// <summary>
        /// Adds the TImplemenation to DI and then adds the TAbstract, ensuring that
        /// the same object is returned within the scope 
        /// </summary>
        public static DiModule AddTransient<TAbstract, TImplementation>(this DiModule module, Func<IServiceProvider, TImplementation>? factory = null)
            where TAbstract : class
            where TImplementation : class, TAbstract
        {
            module = factory == null
                ? module.AddTransient<TImplementation>()
                : module.AddTransient(factory);

            return module
                .AddTransient(x => x.GetService<TImplementation>() as TAbstract ?? throw new Exception("something is not working well"));
        }

        public static DiModule AddTransient<T>(this DiModule module, Func<IServiceProvider, T>? factory = null)
            where T : class
        {
            module = factory == null
                ? module.AddDependencies<T>()
                : module;

            return (DiModule)(factory == null
                ? ((IServiceCollection)module).AddTransient<T>()
                : ((IServiceCollection)module).AddTransient(factory));
        }


        /// <summary>
        /// Register a service and register it's constructor args as dependencies
        /// </summary>
        private static DiModule AddDependencies<T>(this DiModule module)
            where T : class
        {
            var t = typeof(T);
            var constructors = t.GetConstructors();
            if (constructors.Length > 1)
            {
                Console.WriteLine($"Could not determine the dependencies of {typeof(T)} as it has more than 1 constructor");
                return module;
            }

            if (constructors.Length == 1)
            {
                foreach (var param in constructors[0].GetParameters())
                {
                    module.AddDependency(param.ParameterType);
                }
            }

            return module;
        }

        internal static DependencyInjectionModuleState? TryGetDiModuleServiceState(this IServiceCollection services)
        {
            return services
                .Where(x => x.ImplementationInstance?.GetType() == typeof(DependencyInjectionModuleState))
                .Select(x => x.ImplementationInstance as DependencyInjectionModuleState)
                .FirstOrDefault();
        }

        public static IEnumerable<KeyValuePair<Type, IEnumerable<UnresolvedDependency>>> GetUnresolvedModuleDependencies(this IServiceCollection services)
        {
            var serviceState = services.TryGetDiModuleServiceState();

            // there are no modules
            if (serviceState == null)
            {
                yield break;
            }

            var dependencyList = new Lazy<List<(Type dependency, Type dependant)>>(() => BuildDependencyList(services));
            foreach (var (module, dependency) in serviceState.RegisteredModules
                .SelectMany(m => m.Dependencies.Select(d => (m, d)))
                .Distinct())
            {
                // IServiceProvider is always available
                if (dependency == typeof(IServiceProvider))
                {
                    continue;
                }

                if (!services.Any(s => s.ServiceType == dependency))
                {
                    var dependants = dependencyList.Value
                        .Where(d => d.dependency == dependency)
                        .Select(d => new UnresolvedDependency(d.dependant, module.Name))
                        .Distinct();

                    yield return KeyValuePair.Create(dependency, dependants);
                }
            }
        }

        private static List<(Type dependency, Type dependant)> BuildDependencyList(IServiceCollection services)
        {
            return services
                .SelectMany(s => s.ServiceType
                    .GetConstructors()
                    .SelectMany(c => c
                        .GetParameters()
                        .Select(p => (p.ParameterType, s.ServiceType))))
                .ToList();
        }

        public static IServiceCollection MarkDependencyAsNotNeeded<T>(this IServiceCollection collection)
            where T : class
        {
            return collection.AddSingleton(MarkDependencyAsNotNeeded<T>);
        }

        public static DiModule MarkDependencyAsNotNeeded<T>(this DiModule module)
            where T : class
        {
            return module.AddSingleton(MarkDependencyAsNotNeeded<T>);
        }

        private static T MarkDependencyAsNotNeeded<T>(IServiceProvider _)
        {
            throw new InvalidOperationException($"seems like {typeof(T)} is registered as but is not needed");
        }
    }
}

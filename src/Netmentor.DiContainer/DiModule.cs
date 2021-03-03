using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Netmentor.DiContainer
{
    public class DiModule : List<ServiceDescriptor>, IServiceCollection
    {
        private static readonly object _staticLock = new object();
        private readonly List<Type> _dependencies;
        public readonly ReadOnlyCollection<Type> Dependencies;
        private readonly DependencyInjectionModuleState _defaultDiModuleServiceState;
        public readonly string Name;

        public DiModule(Assembly forAssembly)
            : this(forAssembly.GetName().Name ?? "Unknown assembly")
        {
        }

        private DiModule(string name)
        {
            _dependencies = new List<Type>();
            Dependencies = _dependencies.AsReadOnly();
            Name = name;

            _defaultDiModuleServiceState = new DependencyInjectionModuleState();
            Add(new ServiceDescriptor(typeof(DependencyInjectionModuleState), _defaultDiModuleServiceState));
            _defaultDiModuleServiceState.RegisteredModules.Add(this);
        }

        /// <summary>
        /// Create a new module that combines this and another module
        /// </summary>
        public DiModule Concat(DiModule module)
        {
            return new DiModule($"{Name}, {module.Name}")
                .ApplyModule(this)
                .ApplyModule(module);
        }

        /// <summary>
        /// Register a dependency, which must be added to DI before this module is applied
        /// </summary>
        public DiModule AddDependency<TDependency>()
        {
            return AddDependency(typeof(TDependency));
        }

        /// <summary>
        /// Register a dependency, which must be added to DI before this module is applied
        /// </summary>
        public DiModule AddDependency(Type dependency)
        {
            if (dependency == typeof(string))
            {
                throw new InvalidOperationException($"{typeof(string)} is not a valid dependency.");
            }

            _dependencies.Add(dependency);
            return this;
        }

        /// <summary>
        /// Apply the services in this module to another IServiceCollection
        /// Will only be applied a single time to each collection, otherwise does nothing 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection Apply(IServiceCollection services)
        {
            lock (_staticLock)
            {
                return Apply_Unsafe(services);
            }
        }

        private IServiceCollection Apply_Unsafe(IServiceCollection services)
        {
            var serviceState = services.TryGetDiModuleServiceState();
            if (serviceState?.RegisteredModules.Contains(this) ?? false)
            {
                // this module is already registered
                return services;
            }

            // this is the first module to be registered
            if (serviceState == null)
            {
                serviceState = new DependencyInjectionModuleState();
                services.Add(new ServiceDescriptor(typeof(DependencyInjectionModuleState), serviceState));
            }

            serviceState.Combine(_defaultDiModuleServiceState);

            // copy all of the services from this module to the SC
            foreach (var service in this)
            {
                if (service.ImplementationInstance != _defaultDiModuleServiceState)
                {
                    services.Add(service);
                }
            }

            return services;
        }

        bool ICollection<ServiceDescriptor>.IsReadOnly => false;

        IEnumerator<ServiceDescriptor> IEnumerable<ServiceDescriptor>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

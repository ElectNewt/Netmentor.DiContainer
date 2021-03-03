using System.Collections.Generic;

namespace Netmentor.DiContainer
{
    internal class DependencyInjectionModuleState
    {
        public readonly HashSet<DiModule> RegisteredModules = new HashSet<DiModule>();

        public void Combine(DependencyInjectionModuleState Module)
        {
            foreach (var module in Module.RegisteredModules)
            {
                RegisteredModules.Add(module);
            }
        }
    }
}

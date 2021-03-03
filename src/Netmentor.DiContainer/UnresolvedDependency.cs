using System;

namespace Netmentor.DiContainer
{
    public class UnresolvedDependency
    {
        public readonly Type Type;
        public readonly string ModuleName;

        public UnresolvedDependency(Type type, string moduleName)
        {
            Type = type;
            ModuleName = moduleName;
        }
    }
}

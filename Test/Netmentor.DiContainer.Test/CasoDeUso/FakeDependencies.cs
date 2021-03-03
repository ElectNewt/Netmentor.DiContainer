using Netmentor.DiContainer;

namespace Netmentor.DependencyInjection.Test.CasoDeUso
{
    public interface IAbstractionNumber1
    {
        public string GetStringValue();
    }
    public class ImplementationNumber1 : IAbstractionNumber1
    {
        public string GetStringValue()
        {
            return "1";
        }
    }

    public interface IAbstractionNumber2
    {
        public int GetIntValue();
    }
    public class ImplementationNumber2 : IAbstractionNumber2
    {
        public int GetIntValue()
        {
            return 1;
        }
    }


    public static class FakeDependencies1
    {
        public static readonly DiModule DiModule = BuildDependencyInjection();
        private static DiModule BuildDependencyInjection()
        {
            var module = new DiModule(typeof(FakeDependencies1).Assembly);
            return module
                .ApplyModule(FakeDependencies2.DiModule)
                .AddScoped<IAbstractionNumber1, ImplementationNumber1>();
        }
    }

    public static class FakeDependencies2
    {
        public static readonly DiModule DiModule = BuildDependencyInjection();
        private static DiModule BuildDependencyInjection()
        {
            var module = new DiModule(typeof(FakeDependencies2).Assembly);
            return module
                .AddScoped<IAbstractionNumber2, ImplementationNumber2>();
        }
    }

}

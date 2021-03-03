using Microsoft.Extensions.DependencyInjection;
using Netmentor.DiContainer;
using Xunit;

namespace Netmentor.DependencyInjection.Test.CasoDeUso
{
    public class TestDependencyInjectionLibrary
    {

        [Fact]
        public void Test()
        {
            var serviceProvider = new ServiceCollection()
              .ApplyModule(FakeDependencies1.DiModule)
              .BuildServiceProvider();

            //Assert the first dependency
            IAbstractionNumber1 servicioAbstraccion1 = serviceProvider
                .GetService<IAbstractionNumber1>();
            Assert.NotNull(servicioAbstraccion1);
            string result1 = servicioAbstraccion1.GetStringValue();
            Assert.Equal("1", result1);

            //assert the second dependency
            IAbstractionNumber2 servicioAbstraccion2 = serviceProvider
                .GetService<IAbstractionNumber2>();
            Assert.NotNull(servicioAbstraccion2);
            int result2 = servicioAbstraccion2.GetIntValue();
            Assert.Equal(1, result2);

        }

    }
}

using System.Collections.Generic;
using Moq;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForReduceMethodFactoryProvider
    {
        #region Support Classes

        public class TestReduceMethodProvider : ReduceMethodProvider
        {
            public new IDictionary<string, IReduceMethod<double>> Factories
            {
                get { return base.Factories; }
            }
        }

        #endregion

        public class UsingRegisters
        {
            [Fact]
            public void ShouldBeAbleToRegister()
            {
                var method = new Mock<IReduceMethod<double>>();
                var methodInstance = method.Object;

                var factoryProvider = new TestReduceMethodProvider();
                factoryProvider.Register(method.Object);

                Assert.Contains(methodInstance.GetType().FullName, factoryProvider.Factories.Keys);

                method.VerifyAll();
            }
        }

        public class UsingBuilds
        {
            [Fact]
            public void ShouldBeAbleToRetrieve()
            {
                var method = new Mock<IReduceMethod<double>>();
                var methodInstance = method.Object;
                 
                var factoryProvider = new TestReduceMethodProvider();
                factoryProvider.Factories.Add(methodInstance.GetType().FullName, methodInstance);
                var result = factoryProvider.Retrieve(methodInstance.GetType().FullName);

                Assert.Same(methodInstance, result); 
            }

            [Fact]
            public void SHouldThrowExceptionWhenFactoryNotFound()
            {
                var factoryProvider = new TestReduceMethodProvider(); 
                Assert.Throws<KeyNotFoundException>(() => factoryProvider.Retrieve("Test"));
            }
        }
    }
}
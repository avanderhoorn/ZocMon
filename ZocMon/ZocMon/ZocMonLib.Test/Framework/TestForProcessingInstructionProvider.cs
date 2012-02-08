using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForProcessingInstructionProvider
    {
        #region Support Classes

        public class TestProcessingInstructionProvider : ProcessingInstructionProvider
        {
            public new IDictionary<string, IProcessingInstruction> Factories
            {
                get { return base.Factories; }
            }
        }

        #endregion

        public class UsingRegister
        {
            [Fact]
            public void ShouldBeAbleToRegister()
            {
                var instruction = new Mock<IProcessingInstruction>();
                var instructionInstance = instruction.Object;

                var factoryProvider = new TestProcessingInstructionProvider();
                factoryProvider.Register(instruction.Object);

                Assert.Contains(instructionInstance.GetType().FullName, factoryProvider.Factories.Keys);

                instruction.VerifyAll();
            }
        }

        public class UsingBuild
        {
            [Fact]
            public void ShouldBeAbleToBuild()
            {
                var instruction = new Mock<IProcessingInstruction>();
                var instructionInstance = instruction.Object;

                var factoryProvider = new TestProcessingInstructionProvider();
                factoryProvider.Factories.Add(instructionInstance.GetType().FullName, instructionInstance);
                var result = factoryProvider.Retrieve(instructionInstance.GetType().FullName);

                Assert.Same(instructionInstance, result); 
            }

            [Fact]
            public void SHouldThrowExceptionWhenFactoryNotFound()
            {
                var factoryProvider = new TestProcessingInstructionProvider();
                Assert.Throws<KeyNotFoundException>(() => factoryProvider.Retrieve("Test"));
            }
        }
    }
}

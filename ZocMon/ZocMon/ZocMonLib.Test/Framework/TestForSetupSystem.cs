using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib.Extensibility;

namespace ZocMonLib.Test
{
    public class TestForSetupSystem : TestBase
    {
        public class UsingInitialize
        {
            [Fact]
            public void ShouldBeAbleToUse()
            {
                var connection = new Mock<IDbConnection>();
                connection.Setup(x => x.Open()).Verifiable();
                connection.Setup(x => x.Close()).Verifiable();
                var connectionInstance = connection.Object;

                var setupSystemTables = new Mock<ISetupSystemTables>();
                setupSystemTables.Setup(x => x.ValidateAndCreateDataTables(connectionInstance)).Verifiable();

                var setupSystemData = new Mock<ISetupSystemData>();
                setupSystemData.Setup(x => x.LoadAndValidateData(connectionInstance)).Verifiable();

                var storageFactory = new Mock<IStorageFactory>();
                storageFactory.Setup(x => x.CreateConnection()).Returns(connectionInstance).Verifiable();
                 
                var setupSystem = new SetupSystem(setupSystemTables.Object, setupSystemData.Object, storageFactory.Object);
                setupSystem.Initialize();


                connection.VerifyAll();
                storageFactory.VerifyAll();
                setupSystemData.VerifyAll();
                setupSystemTables.VerifyAll();
            }
        }
    }
}

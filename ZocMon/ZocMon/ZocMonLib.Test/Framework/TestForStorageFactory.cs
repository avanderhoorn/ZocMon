using System.Data;
using System.Data.Common;
using Moq;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForStorageFactory
    {
        public class UsingCreateCommand
        {
            [Fact]
            public void CanGetInstance()
            { 
                var commandInstance = new Mock<DbCommand>().Object;

                var dbFactory = new Mock<DbProviderFactory>();
                dbFactory.Setup(x => x.CreateCommand()).Returns(commandInstance).Verifiable();

                var storageFactory = new StorageFactory(dbFactory.Object);
                var result = storageFactory.CreateCommand();

                Assert.Same(commandInstance, result);

                dbFactory.VerifyAll();
            }
        }

        public class UsingCreateCommandOverride
        {
            [Fact]
            public void CanGetInstance()
            {
                var connectionInstance = new Mock<DbConnection>().Object;
                var transactionInstance = new Mock<DbTransaction>().Object;
                
                var command = new Mock<DbCommand>();
                command.SetupSet(x => x.CommandText = "Test").Verifiable();
                command.SetupSet(x => x.Connection = connectionInstance).Verifiable();
                command.SetupSet(x => x.Transaction = transactionInstance).Verifiable();
                var commandInstance = command.Object;

                var dbFactory = new Mock<DbProviderFactory>();
                dbFactory.Setup(x => x.CreateCommand()).Returns(commandInstance).Verifiable(); 

                var storageFactory = new StorageFactory(dbFactory.Object);
                var result = storageFactory.CreateCommand("Test", connectionInstance, transactionInstance);

                Assert.Same(commandInstance, result);

                command.VerifyAll();
                dbFactory.VerifyAll();
            }
        }

        public class UsingCreateConnection
        {
            [Fact]
            public void CanGetInstance()
            {
                var connectionInstance = new Mock<DbConnection>().Object;

                var dbFactory = new Mock<DbProviderFactory>();
                dbFactory.Setup(x => x.CreateConnection()).Returns(connectionInstance).Verifiable();

                var storageFactory = new StorageFactory(dbFactory.Object);
                var result = storageFactory.CreateConnection();

                Assert.Same(connectionInstance, result);

                dbFactory.VerifyAll();
            }
        }

        public class UsingCreateParameter
        {
            [Fact]
            public void CanGetInstance()
            {
                var parameterInstance = new Mock<DbParameter>().Object;

                var dbFactory = new Mock<DbProviderFactory>();
                dbFactory.Setup(x => x.CreateParameter()).Returns(parameterInstance).Verifiable();

                var storageFactory = new StorageFactory(dbFactory.Object);
                var result = storageFactory.CreateParameter();

                Assert.Same(parameterInstance, result);

                dbFactory.VerifyAll();
            }
        }


        public class UsingCreateParameterOverride
        {
            [Fact]
            public void CanGetInstance()
            { 
                var parameter = new Mock<DbParameter>();
                parameter.SetupSet(x => x.DbType = DbType.DateTime).Verifiable();
                parameter.SetupSet(x => x.ParameterName = "Test").Verifiable();
                var parameterInstance = parameter.Object;

                var dbFactory = new Mock<DbProviderFactory>();
                dbFactory.Setup(x => x.CreateParameter()).Returns(parameterInstance).Verifiable();

                var storageFactory = new StorageFactory(dbFactory.Object);
                var result = storageFactory.CreateParameter("Test", DbType.DateTime);

                Assert.Same(parameterInstance, result);

                parameter.VerifyAll();
                dbFactory.VerifyAll();
            }
        }
    }
}

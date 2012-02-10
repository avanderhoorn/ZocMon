using ZocMonLib;

namespace ZocMonLib
{
    public class SetupSystem : ISetupSystem
    {
        private readonly ISetupSystemTables _setupSystemTables;
        private readonly IStorageFactory _storageFactory;
        private readonly ISetupSystemData _setupSystemData;

        public SetupSystem(ISetupSystemTables setupSystemTables, ISetupSystemData setupSystemData, IStorageFactory storageFactory)
        {
            _setupSystemTables = setupSystemTables;
            _setupSystemData = setupSystemData;
            _storageFactory = storageFactory;
        }

        public void Initialize()
        { 
            using (var conn = _storageFactory.CreateConnection())
            {
                conn.Open();

                _setupSystemData.LoadAndValidateData(conn);

                _setupSystemTables.ValidateAndCreateDataTables(conn);
                 
                conn.Close();
            }
        }
    }
}
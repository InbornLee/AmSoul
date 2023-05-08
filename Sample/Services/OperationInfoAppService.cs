using AmSoul.SQL;
using Sample.Models;

namespace Sample.Services
{
    public class OperationInfoAppService : SqlQueryServiceBase<OperationInfo>
    {
        public OperationInfoAppService(MySqlDatabaseSetting settings) : base(settings)
        {
        }
    }
}

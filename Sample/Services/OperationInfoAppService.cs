using AmSoul.Core.Models;
using AmSoul.Extension.Sql.Services;
using Sample.Models;

namespace Sample.Services
{
    public class OperationInfoAppService : SqlSugarQueryServiceBase<OperationInfo>
    {
        public OperationInfoAppService(MySqlDatabaseSetting settings) : base(settings)
        {
        }
    }
}

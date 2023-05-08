using AmSoul.Extension.Sql.Models;
using AmSoul.Extension.Sql.Services;
using Sample.Models;

namespace Sample.Services
{
    public class OperationCostAppService : SqlSugarQueryServiceBase<OperationCost>
    {
        public OperationCostAppService(OracleDatabaseSetting settings) : base(settings) { }
        //public async Task<List<OperationCost>> GetAsync(string id, string secondCategoryCode, CancellationToken cancellationToken = default)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    return await _connection.Queryable<OperationCost>().Where(o => o.Id == id && o.SecondCategoryCode == secondCategoryCode).ToListAsync();
        //}
    }
}

using AmSoul.Core.Interfaces;
using AmSoul.Extension.Sql.Services;
using Sample.Models;

namespace Sample.Services
{
    public class OperationCostAppService : SqlSugarRestServiceBase<OperationCost>
    {
        public OperationCostAppService(IOracleDatabaseSetting settings) : base(settings) { }
        //public async Task<List<OperationCost>> GetAsync(string id, string secondCategoryCode, CancellationToken cancellationToken = default)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    return await _connection.Queryable<OperationCost>().Where(o => o.Id == id && o.SecondCategoryCode == secondCategoryCode).ToListAsync();
        //}
    }
}

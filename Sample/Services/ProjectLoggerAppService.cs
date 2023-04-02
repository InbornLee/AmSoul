using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using AmSoul.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Sample.Models;

namespace Sample.Services;

/// <summary>
/// 日志
/// </summary>
public class ProjectLoggerAppService : MongoRestServiceBase<ProjectLogger>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    public ProjectLoggerAppService(MongoDbDatabaseSetting settings) : base(settings)
    {
    }
    /// <summary>
    /// 按ProductId删除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IBaseResponse> DeleteByProductIdAsync([FromQuery] string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var filter = new Dictionary<string, object>
        {
            { "projectId", id }
        };
        return await base.DeleteAsync(filter, cancellationToken);
    }
}

using AmSoul.Core.Models;
using AmSoul.Core.Services;
using Sample.Models;

namespace Sample.Services;

/// <summary>
/// AppleB
/// </summary>
public class ArchiveAppService : MongoRestServiceBase<Archive>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    public ArchiveAppService(MongoDbDatabaseSetting settings) : base(settings)
    {
    }
}

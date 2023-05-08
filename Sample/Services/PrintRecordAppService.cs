using AmSoul.MongoDB;
using Sample.Models;

namespace Sample.Services;

/// <summary>
/// AppleB
/// </summary>
public class PrintRecordAppService : MongoRestServiceBase<PrintRecord>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    public PrintRecordAppService(MongoDbDatabaseSetting2 settings) : base(settings)
    {
    }
}

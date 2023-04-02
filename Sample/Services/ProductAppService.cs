using Sample.Models;

namespace Sample.Services;

/// <summary>
/// AppleA
/// </summary>
public class ProductAppService : ServiceBase<UpdateAppleDto>
{
    /// <summary>
    /// 更改的新增
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override string Get(int id)
    {
        id = 1;
        return base.Get(id);
    }
}

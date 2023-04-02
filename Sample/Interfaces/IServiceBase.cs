namespace Sample.Interfaces;

/// <summary>
/// 服务接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IServiceBase<T> where T : class
{
    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    string Get(int id);
    /// <summary>
    /// 获取
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> Get();
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="dto"></param>
    void Update(T dto);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    void Delete(int id);
}

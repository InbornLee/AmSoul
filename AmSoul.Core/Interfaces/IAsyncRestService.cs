using AmSoul.Core.Models;

namespace AmSoul.Core.Interfaces;

/// <summary>
/// Restful异步服务基础接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAsyncRestService<T> where T : class, IDataModel
{
    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> CreateAsync(T obj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 按ID查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> GetAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> DeleteAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> PutAsync(string id, T obj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="paginationParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> GetPagesAsync(Pagination paginationParams, CancellationToken cancellationToken = default);
    /// <summary>
    /// 多条件查询
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IBaseResponse> QueryAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    //Task<IBaseResponse> AggregateAsync(List<string> conditions, CancellationToken cancellationToken = default);
}

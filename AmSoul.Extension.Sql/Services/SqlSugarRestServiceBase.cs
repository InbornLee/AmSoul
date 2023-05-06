using AmSoul.Core.Extensions;
using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using AmSoul.Core.Resources;
using Microsoft.AspNetCore.Mvc;
using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;
using SqlSugar;

namespace AmSoul.Extension.Sql.Services
{

    /// <summary>
    /// SqlSugar Restful异步查询服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DynamicWebApi]

    public abstract class SqlSugarQueryServiceBase<T> : IDisposable, IDynamicWebApi, IAsyncQueryService<T> where T : class, IDataModel, new()
    {
        protected readonly SqlSugarClient _connection;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="settings"></param>
        public SqlSugarQueryServiceBase(IDatabaseSetting settings)
        {
            _connection = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = settings.ConnectionString,
                DbType = GetConnectDbType(settings),
                IsAutoCloseConnection = true
            });
        }
        [NonDynamicMethod]
        private DbType GetConnectDbType(IDatabaseSetting settings)
        {
            var type = settings.GetType();
            return type.Name switch
            {
                string s when s.Contains("Sqlite") => DbType.Sqlite,
                string s when s.Contains("PostgreSQL") => DbType.PostgreSQL,
                string s when s.Contains("MySql") => DbType.MySql,
                string s when s.Contains("SqlServer") => DbType.SqlServer,
                string s when s.Contains("Oracle") => DbType.Oracle,
                _ => DbType.Custom,
            };
        }


        /// <summary>
        /// 按ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual async Task<IBaseResponse> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (id == null) throw new ArgumentNullException("id");
                var result = await _connection.Queryable<T>().Where(o => o.Id == id).ToListAsync(cancellationToken);
                return new BaseResponse<List<T>>()
                {
                    Succeeded = result != null && result.Count > 0,
                    Message = result != null && result.Count > 0 ? Resources.QuerySucceeded.Format(result.Count) : Resources.QueryFail,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
        /// <summary>
        /// 多条件查询
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        public virtual async Task<IBaseResponse> QueryAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (parameters == null || parameters.Count < 1) throw new ArgumentException(Resources.QueryParamError);
                var filters = new List<IConditionalModel>();
                foreach (var parameter in parameters)
                {
                    filters.Add(new ConditionalModel() { FieldName = parameter.Key, ConditionalType = ConditionalType.Equal, FieldValue = parameter.Value.ToString() });
                }
                var result = await _connection.Queryable<T>().Where(filters).ToListAsync(cancellationToken);
                return new BaseResponse<List<T>>()
                {
                    Succeeded = result != null && result.Count > 0,
                    Message = result != null && result.Count > 0 ? Resources.QuerySucceeded.Format(result.Count) : Resources.QueryFail,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message },
                };
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="paginationParams"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [HttpPost]
        public virtual async Task<IBaseResponse> GetPagesAsync(Pagination paginationParams, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (paginationParams == null) throw new ArgumentException(Resources.AggregateFail);
                RefAsync<int> total = 0;
                var pagenumber = (paginationParams.Offset / paginationParams.Limit) + 1;
                var result = await _connection.Queryable<T>().ToPageListAsync(pagenumber, paginationParams.Limit, total);
                return new BaseResponse<PageData<T>>
                {
                    Succeeded = true,
                    Message = Resources.QueryComplete,
                    Data = new() { Total = total, Data = result }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        [NonDynamicMethod]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// SqlSugar Restful异步基础服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DynamicWebApi]

    public abstract class SqlSugarRestServiceBase<T> : SqlSugarQueryServiceBase<T>, IAsyncRestService<T> where T : class, IDataModel, new()
    {
        public SqlSugarRestServiceBase(IDatabaseSetting settings) : base(settings) { }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IBaseResponse> CreateAsync(T obj, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (obj == null) throw new ArgumentNullException(nameof(obj));
                var result = await _connection.Insertable(obj).ExecuteReturnEntityAsync();

                return new BaseResponse<T>()
                {
                    Succeeded = true,
                    Message = Resources.CreateSucceeded,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.CreateFail,
                    Errors = new List<string>() { ex.Message },
                    Data = obj
                };
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual async Task<IBaseResponse> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (id == null) throw new ArgumentNullException("id");
                var result = await _connection.Deleteable<T>().Where(o => o.Id == id).ExecuteCommandAsync(cancellationToken);
                return new BaseResponse<int>()
                {
                    Succeeded = result > 0,
                    Message = result > 0 ? Resources.DeleteSucceeded.Format(result) : Resources.DeleteFail.Format(result),
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message },
                };
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [NonDynamicMethod]
        public virtual async Task<IBaseResponse> DeleteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (parameters == null) throw new ArgumentNullException("parameters");
                var filters = new List<IConditionalModel>();
                foreach (var parameter in parameters)
                {
                    filters.Add(new ConditionalModel() { FieldName = parameter.Key, ConditionalType = ConditionalType.Equal, FieldValue = parameter.Value.ToString() });
                }
                var result = await _connection.Deleteable<T>().Where(filters).ExecuteCommandAsync(cancellationToken);
                return new BaseResponse<int>()
                {
                    Succeeded = result > 0,
                    Message = result > 0 ? Resources.DeleteSucceeded.Format(result) : Resources.DeleteFail.Format(result),
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message },
                };
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id:length(24)}")]
        public virtual async Task<IBaseResponse> PutAsync(string id, T obj, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (id == null || obj == null) throw new ArgumentNullException(nameof(obj));
                obj.Id = id;
                var result = await _connection.Updateable(obj).Where(o => o.Id == id).ExecuteCommandAsync();
                return new BaseResponse<int>()
                {
                    Succeeded = result > 0,
                    Message = result > 0 ? Resources.UpdateSucceeded.Format(result) : Resources.UpdateFail.Format(result),
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<T>()
                {
                    Succeeded = false,
                    Message = Resources.Exception,
                    Errors = new List<string>() { ex.Message },
                };
            }
        }

    }

}

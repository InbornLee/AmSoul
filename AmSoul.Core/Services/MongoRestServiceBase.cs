using AmSoul.Core.Extensions;
using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;

namespace AmSoul.Core.Services;

[DynamicWebApi]
public abstract class MongoQueryServiceBase<T> : IDisposable, IDynamicWebApi, IAsyncQueryService<T> where T : class, IDataModel
{
    protected readonly IMongoCollection<T> _collection;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    public MongoQueryServiceBase(IDatabaseSetting settings) => _collection = MongoDbQueryExtensions.GetCollection<T>(settings, $"{typeof(T).Name.ToCamelCase()}s");

    /// <summary>
    /// 按ID查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:length(24)}")]
    public virtual async Task<IBaseResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id == null) throw new ArgumentNullException("id");
            var result = await _collection.FindAsync(obj => obj.Id == id, cancellationToken: cancellationToken);
            var resultList = await result.ToListAsync(cancellationToken: cancellationToken);
            return new BaseResponse<List<T>>()
            {
                Succeeded = resultList != null && resultList.Count > 0,
                Message = resultList != null && resultList.Count > 0 ? Resources.Resources.QuerySucceeded.Format(resultList.Count) : Resources.Resources.QueryFail,
                Data = resultList
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
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
            if (parameters == null || parameters.Count < 1) throw new ArgumentException(Resources.Resources.QueryParamError);
            var filters = parameters.Select(x => Builders<T>.Filter.Eq(x.Key.ToPascalCase(), x.Value)).ToList();
            var result = await _collection.FindAsync(Builders<T>.Filter.And(filters), cancellationToken: cancellationToken);
            var resultList = await result.ToListAsync(cancellationToken: cancellationToken);
            return new BaseResponse<List<T>>()
            {
                Succeeded = resultList != null && resultList.Count > 0,
                Message = resultList != null && resultList.Count > 0 ? Resources.Resources.QuerySucceeded.Format(resultList.Count) : Resources.Resources.QueryFail,
                Data = resultList
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
                Errors = new List<string>() { ex.Message },
            };
        }
    }

    /// <summary>
    /// 聚合查询
    /// </summary>
    /// <param name="conditions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IBaseResponse> AggregateAsync([FromBody] List<string> conditions, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (conditions == null || conditions.Count < 1) throw new ArgumentException(Resources.Resources.AggregateFail);
            var stages = conditions.Select((condition, i) => i == 0
              ? (IPipelineStageDefinition)new JsonPipelineStageDefinition<T, BsonDocument>(condition)
              : new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(condition)).ToList();

            PipelineStagePipelineDefinition<T, BsonDocument> pipeline = new(stages);
            using var result = await _collection.AggregateAsync(pipeline, new AggregateOptions { AllowDiskUse = true }, cancellationToken);
            var list = await result.ToListAsync(cancellationToken: cancellationToken);
            return new BaseResponse<List<dynamic>>()
            {
                Succeeded = true,
                Message = Resources.Resources.QueryComplete,
                Data = list.Select(v => DynamicExtensions.GetDynamicRootObject(BsonSerializer.Deserialize<dynamic>(v))).ToList()
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<string>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
                Errors = new List<string>() { ex.Message }
            };
        }

    }
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="paginationParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IBaseResponse> GetPagesAsync(Pagination paginationParams, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (paginationParams == null) throw new ArgumentException(Resources.Resources.AggregateFail);
            var conditions = paginationParams.ParseMongoAggregateStage();
            var stages = conditions.Select((condition, i) => i == 0
              ? (IPipelineStageDefinition)new BsonDocumentPipelineStageDefinition<T, BsonDocument>(condition.ToBsonDocument())
              : new BsonDocumentPipelineStageDefinition<BsonDocument, BsonDocument>(condition.ToBsonDocument()))
                .ToList();

            PipelineStagePipelineDefinition<T, BsonDocument> pipeline = new(stages);
            var result = (await _collection.AggregateAsync(pipeline, new AggregateOptions { AllowDiskUse = true }, cancellationToken)).FirstOrDefault(cancellationToken: cancellationToken);

            if (result == null) return new PageData<T>() { Succeeded = false, Message = Resources.Resources.QueryFail };
            var data = BsonSerializer.Deserialize<PageData<T>>(result);
            data.Succeeded = true;
            data.Message = Resources.Resources.QueryComplete;
            return data;
        }
        catch (Exception ex)
        {
            return new BaseResponse<string>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
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
/// MongoDB Restful异步基础服务
/// </summary>
/// <typeparam name="T"></typeparam>
[DynamicWebApi]
public abstract class MongoRestServiceBase<T> : MongoQueryServiceBase<T>, IAsyncRestService<T> where T : class, IDataModel
{
    //private readonly IMongoCollection<T> _collection;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    public MongoRestServiceBase(IDatabaseSetting settings) : base(settings) { }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IBaseResponse> CreateAsync(T obj, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            obj.Id = null;
            await _collection.InsertOneAsync(obj, cancellationToken: cancellationToken);

            return new BaseResponse<T>()
            {
                Succeeded = true,
                Message = Resources.Resources.CreateSucceeded,
                Data = obj
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.CreateFail,
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
    [HttpDelete("{id:length(24)}")]
    public virtual async Task<IBaseResponse> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id == null) throw new ArgumentNullException("id");
            var result = await _collection.DeleteOneAsync(o => o.Id == id, cancellationToken: cancellationToken);
            return new BaseResponse<DeleteResult>()
            {
                Succeeded = result.IsAcknowledged && result.DeletedCount > 0,
                Message = result.IsAcknowledged && result.DeletedCount > 0 ? Resources.Resources.DeleteSucceeded.Format(result.DeletedCount) : Resources.Resources.DeleteFail.Format(result.DeletedCount),
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
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
    public virtual async Task<IBaseResponse> DeleteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (parameters == null) throw new ArgumentNullException("parameters");
            var filters = parameters.Select(x => Builders<T>.Filter.Eq(x.Key.ToPascalCase(), x.Value))
                                    .ToList();
            var result = await _collection.DeleteManyAsync(Builders<T>.Filter.And(filters), cancellationToken: cancellationToken);

            return new BaseResponse<DeleteResult>()
            {
                Succeeded = result.IsAcknowledged && result.DeletedCount > 0,
                Message = result.IsAcknowledged && result.DeletedCount > 0 ? Resources.Resources.DeleteSucceeded.Format(result.DeletedCount) : Resources.Resources.DeleteFail.Format(result.DeletedCount),
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
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
    public virtual async Task<IBaseResponse> PutAsync(string id, T obj, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id == null || obj == null) throw new ArgumentNullException(nameof(obj));
            obj.Id = id;
            var result = await _collection.ReplaceOneAsync(o => o.Id == id, obj, cancellationToken: cancellationToken);
            return new BaseResponse<ReplaceOneResult>()
            {
                Succeeded = result.IsAcknowledged && result.ModifiedCount > 0,
                Message = result.IsAcknowledged && result.ModifiedCount > 0 ? Resources.Resources.UpdateSucceeded.Format(result.ModifiedCount) : Resources.Resources.UpdateFail.Format(result.ModifiedCount),
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>()
            {
                Succeeded = false,
                Message = Resources.Resources.Exception,
                Errors = new List<string>() { ex.Message },
            };
        }
    }

}

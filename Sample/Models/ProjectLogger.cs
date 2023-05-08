using AmSoul.Core;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Sample.Models;

/// <summary>
/// 日志
/// </summary>
public class ProjectLogger : IDataModel
{
    /// <summary>
    /// ID
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    /// <summary>
    /// 项目ID
    /// </summary>
    public string? ProjectId { get; set; }
    /// <summary>
    /// 操作人
    /// </summary>
    public string? Operator { get; set; }
    /// <summary>
    /// 操作人ID
    /// </summary>
    public string? OperatorId { get; set; }
    /// <summary>
    /// 操作时间
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime OperateTime { get; set; }
    /// <summary>
    /// 操作内容
    /// </summary>
    public string? Content { get; set; }
}

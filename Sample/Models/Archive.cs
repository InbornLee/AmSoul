using AmSoul.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Sample.Models;

/// <summary>
/// 目录
/// </summary>
public class Archive : IDataModel
{
    public const string CollectionName = "project.archives";

    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Parent { get; set; }
    public string? Text { get; set; }
    public string? Icon { get; set; }
    public bool Children { get; set; }
    public bool LazyLoad { get; set; }
    [BsonIgnore]
    public List<string> Tags = new();
    [BsonIgnore]
    public List<Archive> Nodes = new();
}

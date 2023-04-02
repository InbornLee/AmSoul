using System.ComponentModel.DataAnnotations;

namespace AmSoul.Core.Interfaces;

public interface IDataModel
{
    /// <summary>
    /// ID
    /// </summary>
    [Key]
    //[BsonId]
    //[DataMember]
    //[JsonProperty(Order = 1)]
    //[BsonElement(Order = 0)]
    //[BsonRepresentation(BsonType.ObjectId)]
    string? Id { get; set; }
}

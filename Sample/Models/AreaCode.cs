using AmSoul.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.Models
{
    public class AreaCode : IDataModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? AdCode { get; set; }
        public string? PCode { get; set; }
        public string? Level { get; set; }
    }
}

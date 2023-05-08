using AmSoul.Core;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Sample.Models
{
    public class AreaCode : IDataModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? AdCode { get; set; }
        public string? CityCode { get; set; }
        public string? PCode { get; set; }
        public string? Level { get; set; }
    }
}

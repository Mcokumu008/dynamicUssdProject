using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dynamicUssdProject.Models
{
    public class UserPin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("PhoneNumber")]
        public string PhoneNumber { get; set; }  

        [BsonElement("PinHash")]
        public string PinHash { get; set; } 
    }
}

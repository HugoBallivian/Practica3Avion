using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AvionesDistribuidos.Models
{
    public class PasajeroMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }
    }
}

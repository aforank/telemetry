using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Booking.API
{
    public class BookingDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public Customer Customer { get; set; }

        public Driver Driver { get; set; }

        public double Price { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Feedback_Services.Models
{
	public class FeedbackModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }
		public required int Rating { get; set; }
		public string? Feedback { get; set; }
		public string? Customer { get; set; }
		public required string Product { get; set; }
		public required string Vendor { get; set; }
	}
}
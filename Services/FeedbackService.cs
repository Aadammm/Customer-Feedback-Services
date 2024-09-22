using Customer_Feedback_Services.Configuration;
using Customer_Feedback_Services.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Customer_Feedback_Services.Services
{
	public class FeedbackService
	{
		private readonly IMongoCollection<FeedbackModel> _feedbacksCollection;
		public FeedbackService(IOptions<FeedbackDatabaseSettings> feedBackDatabaseSettings)
		{
			var mongoClient = new MongoClient(
				feedBackDatabaseSettings.Value.ConnectionString);

			var mongoDb = mongoClient.GetDatabase(
				feedBackDatabaseSettings.Value.DatabaseName);

			_feedbacksCollection = mongoDb.GetCollection<FeedbackModel>(
				feedBackDatabaseSettings.Value.CollectionName);
		}

		public IEnumerable<FeedbackModel> GetDocuments() =>
			 _feedbacksCollection.Find(_ => true).ToList();

		public List<FeedbackModel> GetDocuments( string name, int? rating, string produkt)
		{
			var filter = CreateFilter(name, rating, produkt);
			return  _feedbacksCollection.Find(filter).ToList();
		}
		private FilterDefinition<FeedbackModel> CreateFilter(string name, int? rating, string produkt)
		{
			var builder = Builders<FeedbackModel>.Filter;
			var filter = builder.Empty;
			if (!string.IsNullOrWhiteSpace(name))
			{
				filter &= builder.Eq(x => x.Customer, name);
			}
			if (!string.IsNullOrWhiteSpace(produkt))
			{
				filter &= builder.Eq(x => x.Product, produkt);
			}
			if (rating is not null)
			{
				filter &= builder.Eq(x => x.Rating, rating.Value);
			}
			return filter;
		}
		public FeedbackModel GetDocumentById(string id) =>
			 _feedbacksCollection.Find(x => x.Id == id).FirstOrDefault();

		public void CreateDocument(FeedbackModel newFeedBackModel) =>
			 _feedbacksCollection.InsertOne(newFeedBackModel);

		public void Update(string id, FeedbackModel updateFeedback) =>
			_feedbacksCollection.ReplaceOne(x => x.Id == id, updateFeedback);

		public void Remove(string id) =>
		 _feedbacksCollection.DeleteOne(x => x.Id == id);

		public long CountDocuments() =>
			_feedbacksCollection.CountDocuments(new BsonDocument());

	}
}
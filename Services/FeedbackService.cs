using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

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

	public IEnumerable<FeedbackModel> GetDocuments(int pageNumber, int documentsLimitPerPage, string name, int? rating)
	{
		int skipDocuments = pageNumber == 1 ? 0 : (pageNumber * documentsLimitPerPage) - 1;
		var filter = CreateFilter(name, rating);
		return _feedbacksCollection.Find(filter)
			.Skip(skipDocuments)
			.Limit(documentsLimitPerPage)
			.SortByDescending(feedback => feedback.Id)
			.ToList();
	}
	private FilterDefinition<FeedbackModel> CreateFilter   (string name, int? rating)
	{
		var builder = Builders<FeedbackModel>.Filter;
		var filter = builder.Empty;
		if (!string.IsNullOrWhiteSpace(name))
		{
			filter &= builder.Eq(x => x.Customer, name);
		}
		else if (rating is not null)
		{
			filter &= builder.Eq(x => x.Rating, rating);
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


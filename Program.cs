using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<FeedbackDatabaseSettings>(
	builder.Configuration.GetSection("FeedbackDatabase"));

builder.Services.AddSingleton<FeedbackService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

[ApiController]
[Route("[controller]")]
public class FeedbackController : ControllerBase
{
	private readonly FeedbackService _feedbackService;

	public FeedbackController(FeedbackService feedbackService) =>
		_feedbackService = feedbackService;

	[HttpGet]
	public PageResult Get(int pageNumber = 1, int limitPerPage = 10, string customerName = "", int? rating = null)
	{
		long totalDocuments = _feedbackService.CountDocuments();
		int documentsLimit = PaginationHelper.ValidateLimitPerPage(limitPerPage);

		int totalPages = PaginationHelper.CalculateTotalPages(totalDocuments, documentsLimit);
		int page = PaginationHelper.ValidatePageNumber(pageNumber, totalPages);

		bool isFirst = PaginationHelper.IsFirstPage(pageNumber, totalPages);
		bool isLast = PaginationHelper.IsLastPage(pageNumber, totalPages);

		var documents = _feedbackService.GetDocuments(page, documentsLimit, customerName, rating).ToList();
		var countCoduments = documents.Count;
		return new PageResult(countCoduments, isFirst, isLast, documents);
	}

	[HttpGet("{id:length(24)}")]
	public ActionResult<FeedbackModel> Get(string id)
	{
		var feedbackModel = _feedbackService.GetDocumentById(id);

		if (feedbackModel is null)
		{
			return NotFound();
		}
		return feedbackModel;
	}

	[HttpPost]
	public IActionResult Post(FeedbackModel newFeedback)//rating moze byt 1 az 5 
	{
		_feedbackService.CreateDocument(newFeedback);
		return CreatedAtAction(nameof(Get), new { id = newFeedback.Id }, newFeedback);
	}

	[HttpPut]
	[HttpPut("{id:length(24)}")]
	public IActionResult Update(string id, FeedbackModel updatedFeedback)
	{
		var feedback = _feedbackService.GetDocumentById(id);
		if (feedback is null)
		{
			return NotFound();
		}

		updatedFeedback.Id = feedback.Id;

		_feedbackService.Update(id, updatedFeedback);
		return NoContent();
	}

	[HttpDelete("{id:length(24)}")]
	public IActionResult Delete(string id)
	{
		var feedback = _feedbackService.GetDocumentById(id);
		if (feedback is null)
		{
			return NotFound();
		}

		_feedbackService.Remove(id);
		return NoContent();
	}
}

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
		var filter = FilterByNameAndRating(name, rating);
		return _feedbacksCollection.Find(filter)
			.Skip(skipDocuments)
			.Limit(documentsLimitPerPage)
			.SortByDescending(feedback => feedback.Id)
			.ToList();
	}
	private FilterDefinition<FeedbackModel> FilterByNameAndRating(string name, int? rating)
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

public class FeedbackDatabaseSettings
{
	public string ConnectionString { get; set; } = null!;

	public string DatabaseName { get; set; } = null!;

	public string CollectionName { get; set; } = null!;
}
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
public class PageResult(long totalDocuments, bool IsFirst, bool IsLast, List<FeedbackModel> documents)
{
	public long TotalDocuments { get; set; } = totalDocuments;
	public bool IsFirstPage { get; set; } = IsFirst;
	public bool IsLastPage { get; set; } = IsLast;
	public List<FeedbackModel> Documents { get; set; } = documents;
}
public static class PaginationHelper
{
	public static int ValidatePageNumber(int page, int totalPages)
	{
		int correctPage = 1;
		if (page > totalPages)
		{
			correctPage = totalPages;
		}
		else if (page < 1)
		{
			correctPage = 1;
		}
		return correctPage;
	}

	public static int ValidateLimitPerPage(int documentsLimitPerPage)
	{
		int correctNumberOfDocuments = documentsLimitPerPage switch
		{
			> 20 => 20,
			< 5 => 5,
			_ => documentsLimitPerPage,
		};
		return correctNumberOfDocuments;

	}

	public static bool IsFirstPage(int pageNumber, int totalPages) => pageNumber == 1 || totalPages == 1;
	public static bool IsLastPage(int pageNumber, int totalPages) => pageNumber >= totalPages;
	public static int CalculateTotalPages(long totalDocuments, int documentsLimit) => (int)Math.Ceiling((double)totalDocuments / documentsLimit);
}


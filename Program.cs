using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<FeedBackDatabaseSettings>(
	builder.Configuration.GetSection("FeedbackDatabase"));

builder.Services.AddSingleton<FeedBackService>();
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
	private readonly FeedBackService _feedbackService;
	public FeedbackController(FeedBackService feedbackService) =>
		_feedbackService = feedbackService;

	[HttpGet]
	[Route("")]
	[Route("page={pageNumber}")]
	[Route("page={pageNumber}&perPage={documentsLimitPerPage}")]
	public Page Get(int pageNumber, int documentsLimitPerPage = 10)
	{
		long totalDocuments = _feedbackService.CountDocuments();
		int documentsLimit = FilterValidator.NumberOfDocumentsPerPageValidator(documentsLimitPerPage);

		int totalPages = FilterValidator.CountPages(totalDocuments, documentsLimit);
		int page = FilterValidator.PageNumberValidator(pageNumber, totalPages);

		bool isFirst = FilterValidator.IsFirst(pageNumber, totalPages);
		bool isLast = FilterValidator.IsLast(pageNumber, totalPages);

		var documents = _feedbackService.GetLimitedDocuments(page, documentsLimit).ToList();

		return new Page(totalDocuments, isFirst, isLast, documents);
	}

	[HttpGet("{id:length(24)}")]
	public ActionResult<FeedBackModel> Get(string id)
	{
		var feedbackModel = _feedbackService.GetDocumentById(id);

		if (feedbackModel is null)
		{
			return NotFound();
		}
		return feedbackModel;
	}

	[HttpPost]
	public IActionResult Post(FeedBackModel newFeedback)
	{
		_feedbackService.CreateDocument(newFeedback);
		return CreatedAtAction(nameof(Get), new { id = newFeedback.Id }, newFeedback);
	}

	[HttpPut]
	[HttpPut("{id:length(24)}")]
	public IActionResult Update(string id, FeedBackModel updatedFeedback)
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

		_feedbackService.RemoveAsync(id);
		return NoContent();
	}
}

public class FeedBackService
{
	private readonly IMongoCollection<FeedBackModel> _feedbacksCollection;
	public FeedBackService(IOptions<FeedBackDatabaseSettings> feedBackDatabaseSettings)
	{
		var mongoClient = new MongoClient(
			feedBackDatabaseSettings.Value.ConnectionString);

		var mongoDb = mongoClient.GetDatabase(
			feedBackDatabaseSettings.Value.DatabaseName);

		_feedbacksCollection = mongoDb.GetCollection<FeedBackModel>(
			feedBackDatabaseSettings.Value.CollectionName);
	}

	public IEnumerable<FeedBackModel> GetDocuments() =>
		 _feedbacksCollection.Find(FeedBackModel => true).ToList();

	public IEnumerable<FeedBackModel> GetLimitedDocuments(int pageNumber, int documentsLimitPerPage)
	{
		int skipDocumentsCalculate = pageNumber == 1 ? 0 : (pageNumber * documentsLimitPerPage) - 1;
		return _feedbacksCollection.Find(FeedBackModel => true).Skip(skipDocumentsCalculate).Limit(documentsLimitPerPage)
			.SortByDescending(feedback => feedback.Id).ToList();
	}

	public FeedBackModel GetDocumentById(string id) =>
		 _feedbacksCollection.Find(x => x.Id == id).FirstOrDefault();

	public void CreateDocument(FeedBackModel newFeedBackModel) =>
		 _feedbacksCollection.InsertOne(newFeedBackModel);

	public void Update(string id, FeedBackModel updateFeedback) =>
		_feedbacksCollection.ReplaceOne(x => x.Id == id, updateFeedback);

	public void RemoveAsync(string id) =>
	 _feedbacksCollection.DeleteOne(x => x.Id == id);

	public long CountDocuments() =>
		_feedbacksCollection.CountDocuments(new BsonDocument());

}

public class FeedBackDatabaseSettings
{
	public string ConnectionString { get; set; } = null!;

	public string DatabaseName { get; set; } = null!;

	public string CollectionName { get; set; } = null!;
}
public class FeedBackModel
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
public class Page(long totalDocuments, bool IsFirst, bool IsLast, List<FeedBackModel> documents)
{
	public long TotalDocuments { get; set; } = totalDocuments;
	public bool IsFirstPage { get; set; } = IsFirst;
	public bool IsLastPage { get; set; } = IsLast; 
	public List<FeedBackModel> Documents { get; set; } = documents;
}
public static class FilterValidator
{
	public static int PageNumberValidator(int page, int totalPages)
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

	public static int NumberOfDocumentsPerPageValidator(int documentsLimitPerPage)
	{
		int correctNumberOfDocuments = documentsLimitPerPage switch
		{
			> 20 => 20,
			< 5 => 5,
			_ => documentsLimitPerPage,
		};
		return correctNumberOfDocuments;

	}

	public static bool IsFirst(int pageNumber, int totalPages) => pageNumber == 1 || totalPages == 1;
	public static bool IsLast(int pageNumber, int totalPages) => pageNumber >= totalPages;
	public static int CountPages(long totalDocuments, int documentsLimit) => (int)Math.Ceiling((double)totalDocuments / documentsLimit);
}
//ked sa zada page vacsi ako pocetPage zobrazi sa posledna a ak je posledna zaroven prva musi byt atribut true a musia sa zobrazit aj dokumenty


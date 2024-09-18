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
	public async Task<List<FeedBackModel>> Get() =>
	await _feedbackService.GetAsync();


	[HttpGet("{id:length(24)}")]
	public async Task<ActionResult<FeedBackModel>> Get(string id)
	{
		var feedbackModel = await _feedbackService.GetByIdAsync(id);

		if (feedbackModel is null)
		{
			return NotFound();
		}
		return feedbackModel;
	}

	[HttpPost]
	public async Task<IActionResult> Post(FeedBackModel newFeedback)
	{
		await _feedbackService.CreateAsync(newFeedback);
		return CreatedAtAction(nameof(Get), new { id = newFeedback.Id }, newFeedback);
	}

	[HttpPut]
	[HttpPut("{id:length(24)}")]
	public async Task<IActionResult> Update(string id, FeedBackModel updatedFeedback)
	{
		var feedback = await _feedbackService.GetByIdAsync(id);
		if (feedback is null)
		{
			return NotFound();
		}

		updatedFeedback.Id = feedback.Id;

		await _feedbackService.UpdateAsync(id, updatedFeedback);
		return NoContent();
	}

	[HttpDelete("{id:length(24)}}")]
	public async Task<IActionResult> Delete(string id)
	{
		var feedback = _feedbackService.GetByIdAsync(id);
		if (feedback is null)
		{
			return NotFound();
		}

		await _feedbackService.RemoveAsync(id);
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

	public async Task<List<FeedBackModel>> GetAsync() =>
		await _feedbacksCollection.Find(FeedBackModel => true).ToListAsync();

	public async Task<FeedBackModel> GetByIdAsync(string id) =>
		await _feedbacksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

	public async Task CreateAsync(FeedBackModel newFeedBackModel) =>
		await _feedbacksCollection.InsertOneAsync(newFeedBackModel);

	public async Task UpdateAsync(string id, FeedBackModel updateFeedback) =>
		await _feedbacksCollection.ReplaceOneAsync(x => x.Id == id, updateFeedback);

	public async Task RemoveAsync(string id) =>
	await _feedbacksCollection.DeleteOneAsync(x => x.Id == id);


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
public class Page
{
	public long TotalDocuments
	{
		get; set;
	}
	public bool IsFirstPage { get; set; }
	public bool IsLastPage { get; set; }
	public List<FeedBackModel> Documents { get; set; }

}
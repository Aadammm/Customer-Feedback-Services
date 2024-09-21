using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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
	public ActionResult<FeedbackModel> Post(FeedbackModel newFeedback)
	{
		_feedbackService.CreateDocument(newFeedback);
		if (newFeedback.Rating > 5 || newFeedback.Rating < 0)
		{
			return BadRequest("Rating must be within the correct range from 0 to 5");
		}
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


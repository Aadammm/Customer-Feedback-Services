using Customer_Feedback_Services.Models;
using Customer_Feedback_Services.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Customer_Feedback_Services.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FeedbackController : ControllerBase
	{
		private readonly FeedbackService _feedbackService;

		public FeedbackController(FeedbackService feedbackService) =>
			_feedbackService = feedbackService;

		[HttpGet]
		public PageResult Get(int page = 1, int perPage = 10, string name = "", int? rating = null, string produkt = "")
		{
			var documents = _feedbackService.GetDocuments(name, rating, produkt);
			var countCoduments = documents.Count;

			int documentsLimit = PaginationHelper.ValidateLimitPerPage(perPage);
			int totalPages = PaginationHelper.CalculateTotalPages(countCoduments, documentsLimit);
			int pageNumber = PaginationHelper.ValidatePageNumber(page, totalPages);

			bool isFirst = PaginationHelper.IsFirstPage(pageNumber, totalPages);
			bool isLast = PaginationHelper.IsLastPage(pageNumber, totalPages);

			var result = documents.Skip((pageNumber-1 )* documentsLimit)
					.Take(documentsLimit)
					.OrderByDescending(x=>x.Id).ToList();

			return new PageResult(countCoduments, isFirst, isLast, result);
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
}
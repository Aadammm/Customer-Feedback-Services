namespace Customer_Feedback_Services.Models
{
	public class PageResult(long totalDocuments, bool IsFirst, bool IsLast, List<FeedbackModel> documents)
	{
		public long TotalDocuments { get; set; } = totalDocuments;
		public bool IsFirstPage { get; set; } = IsFirst;
		public bool IsLastPage { get; set; } = IsLast;
		public IEnumerable<FeedbackModel> Documents { get; set; } = documents;
	}
}
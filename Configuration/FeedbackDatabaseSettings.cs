namespace Customer_Feedback_Services.Configuration
{
	public class FeedbackDatabaseSettings
	{
		public string ConnectionString { get; set; } = null!;

		public string DatabaseName { get; set; } = null!;

		public string CollectionName { get; set; } = null!;
	}
}
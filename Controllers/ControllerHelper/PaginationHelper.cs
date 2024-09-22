public static class PaginationHelper
{
	public static int ValidatePageNumber(int page, int totalPages)
	{
		if (page > totalPages)
		{
			page= totalPages;
		}
		 if (page < 1)
		{
			page =1;
		}
		return page;
	}

	public static int ValidateLimitPerPage(int perPageLimit)
	{
		int correctNumberOfDocuments = perPageLimit switch
		{
			> 20 => 20,
			< 5 => 5,
			_ => perPageLimit,
		};
		return correctNumberOfDocuments;

	}

	public static bool IsFirstPage(int? pageNumber, int totalPages) => pageNumber == 1 ;
	public static bool IsLastPage(int? pageNumber, int totalPages) => pageNumber >= totalPages;
	public static int CalculateTotalPages(long totalDocuments, int documentsLimit) => (int)Math.Ceiling((double)totalDocuments / documentsLimit);
}


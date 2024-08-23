namespace Library.ViewModels
{
    public class PagedResult<T>
    {
        public List<T> Results { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public PagedResult(List<T> results, int count, int pageNumber, int pageSize)
        {
            Results = results;
            TotalCount = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }

}

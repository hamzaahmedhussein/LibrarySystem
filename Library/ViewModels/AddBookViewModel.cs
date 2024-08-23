namespace Library.ViewModels
{
    public class AddBookViewModel
    {
        public string Title { get; set; }
        public IFormFile Picture { get; set; }
        public string Author { get; set; }
        public int PublishedYear { get; set; }
        public int NumberOfBooks { get; set; }
        public int NumberOfAvailableBooks { get; set; }
        public string Description { get; set; }


    }
}

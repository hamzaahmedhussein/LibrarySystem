namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Picture { get; set; }
        public string Author { get; set; }
        public int PublishedYear { get; set; }
        public int NumberOfBooks { get; set; }
        public int NumberOfAvailableBooks { get; set; }
        public string Description { get; set; }

        public string LibraryOwnerId { get; set; }
        public LibraryOwner LibraryOwner { get; set; }

        public ICollection<BorrowingRecord> BorrowingRecords { get; set; }
    }

}
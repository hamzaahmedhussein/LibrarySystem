namespace Library.ViewModels
{
    public class BorrowBookViewModel
    {
        public string Name { get; set; }
        public string NationalId { get; set; }
        public int BookId { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}

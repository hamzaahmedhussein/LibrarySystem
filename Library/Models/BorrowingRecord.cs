namespace Library.Models
{
    public class BorrowingRecord
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public int BorrowerId { get; set; }
        public Borrower Borrower { get; set; }

        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

}
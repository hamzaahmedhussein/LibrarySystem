namespace Library.Models
{
    public class Borrower
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NationalId { get; set; }
        public ICollection<BorrowingRecord> BorrowingRecords { get; set; }
    }
}
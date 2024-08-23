using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Models
{
    public class ApplicationDbContext : IdentityDbContext<LibraryOwner>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets representing tables in the database
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API configurations if needed
            modelBuilder.Entity<LibraryOwner>()
                .HasMany(l => l.Books)
                .WithOne(b => b.LibraryOwner)
                .HasForeignKey(b => b.LibraryOwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.BorrowingRecords)
                .WithOne(br => br.Book)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Borrower>()
                .HasMany(b => b.BorrowingRecords)
                .WithOne(br => br.Borrower)
                .HasForeignKey(br => br.BorrowerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Borrower>()
                .HasIndex(b => b.NationalId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}

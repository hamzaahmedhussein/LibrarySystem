using Microsoft.AspNetCore.Identity;

namespace Library.Models
{
    public class LibraryOwner : IdentityUser
    {
        public ICollection<Book> Books { get; set; }
    }

}

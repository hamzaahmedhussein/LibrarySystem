using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{

    public class BookController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<LibraryOwner> _userManager;

        public BookController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context, UserManager<LibraryOwner> userManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookViewModel model)
        {
            if (ModelState.IsValid)
            {


                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "LibraryOwner");

                }
                var picturePath = await AddImage(model.Picture);
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Picture = picturePath,
                    Description = model.Description,
                    NumberOfBooks = model.NumberOfBooks,
                    NumberOfAvailableBooks = model.NumberOfBooks,
                    PublishedYear = model.PublishedYear,
                    LibraryOwnerId = currentUser.Id,
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction("getallbooks", "book");
            }
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllBooks(int pageNumber = 1, int pageSize = 5)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "LibraryOwner");
            }

            var totalBooksCount = await _context.Books.CountAsync();
            var books = await _context.Books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Picture = b.Picture,
                    NumberOfBooks = b.NumberOfBooks,
                    NumberOfAvailableBooks = b.NumberOfAvailableBooks,
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalBooksCount / (double)pageSize);

            var viewModel = new PagedResult<BookViewModel>(books, totalBooksCount, pageNumber, pageSize);
            viewModel.TotalPages = totalPages;

            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> BorrowBook(int id)
        {

            var model = new BorrowBookViewModel
            {
                BookId = id,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BorrowBook(BorrowBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var book = await _context.Books.FindAsync(model.BookId);
            if (book == null)
            {
                ModelState.AddModelError("", "The book no longer exists.");
                return View(model);
            }

            if (book.NumberOfAvailableBooks <= 0)
            {
                ModelState.AddModelError("", "The book is not available for borrowing.");
                return View(model);
            }

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.NationalId == model.NationalId);

            if (borrower == null)
            {
                borrower = new Borrower
                {
                    Name = model.Name,
                    NationalId = model.NationalId
                };

                _context.Borrowers.Add(borrower);
                await _context.SaveChangesAsync();
            }

            var borrowingRecord = new BorrowingRecord
            {
                BookId = book.Id,
                BorrowerId = borrower.Id,
                BorrowDate = DateTime.UtcNow,
                ReturnDate = model.ReturnDate
            };

            _context.BorrowingRecords.Add(borrowingRecord);

            // Update book availability
            book.NumberOfAvailableBooks--;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "You have successfully borrowed the book.";
            return RedirectToAction("GetAllBooks", "book");
        }


        [HttpGet]
        public async Task<IActionResult> ReturnBook(int id)
        {


            var model = new ReturnBookViewModel
            {
                BookId = id,
                ReturnDate = DateTime.UtcNow
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(ReturnBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var borrowingRecord = await _context.BorrowingRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.BookId == model.BookId && br.Borrower.NationalId == model.NationalId);

            if (borrowingRecord == null)
            {
                ModelState.AddModelError("", "No matching borrowing record found for the provided Book ID and National ID.");
                return View(model);
            }


            if (borrowingRecord.Book.NumberOfAvailableBooks < borrowingRecord.Book.NumberOfBooks)
            {
                borrowingRecord.Book.NumberOfAvailableBooks++;
            }
            _context.BorrowingRecords.Remove(borrowingRecord);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetAllBooks", "Book");
        }






        #region Image Management
        public async Task<string> AddImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.", nameof(file));
            }

            string rootPath = _webHostEnvironment.WebRootPath;

            string profileFolderPath = Path.Combine(rootPath, "Images", "Books");

            if (!Directory.Exists(profileFolderPath))
            {
                Directory.CreateDirectory(profileFolderPath);
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(profileFolderPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/Images/Books/{fileName}";
        }


        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentException("Image path is null or empty.", nameof(imagePath));
            }

            string rootPath = _webHostEnvironment.WebRootPath;


            if (!imagePath.StartsWith($"/Images/Books/"))
            {
                throw new ArgumentException("Invalid image path.", nameof(imagePath));
            }

            string filePath = Path.Combine(rootPath, imagePath.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            else
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

        }

        public async Task<string> UpdateImageAsync(IFormFile? file, string oldImagePath, string folderName)
        {
            await DeleteImageAsync(oldImagePath);
            string newImagePath = await AddImage(file);
            return newImagePath;
        }
        #endregion

    }
}

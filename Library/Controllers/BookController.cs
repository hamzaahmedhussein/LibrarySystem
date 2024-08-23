using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                if (model.NumberOfAvailableBooks > model.NumberOfBooks)
                {
                    ModelState.AddModelError("", "The number of available books cannot be greater than the total number of books.");
                    return View(model);
                }

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
                    PublishedYear = model.PublishedYear,
                    NumberOfAvailableBooks = model.NumberOfAvailableBooks,
                    LibraryOwnerId = currentUser.Id,
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
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

            var books = _context.Books.Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .Select(b => new BookViewModel
                                 {
                                     Id = b.Id,
                                     Title = b.Title,
                                     Picture = b.Picture,
                                     NumberOfBooks = b.NumberOfBooks,
                                     NumberOfAvailableBooks = b.NumberOfAvailableBooks,
                                 }).ToList();
            var viewModel = new PagedResult<BookViewModel>(books, books.Count(), pageNumber, pageSize);
            return View(viewModel);
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

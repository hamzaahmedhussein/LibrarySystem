using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    public class LibraryOwnerController : Controller
    {

        private readonly UserManager<LibraryOwner> _userManager;
        private readonly SignInManager<LibraryOwner> _signInManager;
        private readonly ApplicationDbContext _context;

        public LibraryOwnerController(UserManager<LibraryOwner> userManager, SignInManager<LibraryOwner> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new LibraryOwner { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("getallbooks", "book");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View("Register", model);

        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.isPersistent, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("getallbooks", "book");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            }
            return View(model);

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login", "libraryOwner");
        }


    }
}

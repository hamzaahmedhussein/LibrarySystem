using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class RegisterViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and Confirmation Password do not match.")]

        public string ConfirmPassword { get; set; }
        public string UserName { get; set; }


    }
}

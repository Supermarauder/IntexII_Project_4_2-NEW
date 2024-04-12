using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntexII_Project_4_2.Controllers
{
    public class AuthUserController : Controller
    {
        [Authorize(Roles = "Member")]
        public IActionResult Checkout()
        {
            return View();
        }
        [Authorize(Roles = "Member")]
        public IActionResult Confirmation0()
        {
            return View();
        }
        [Authorize(Roles = "Member")]
        public IActionResult Confirmation1()
        {
            return View();
        }
        public IActionResult RegistrationSuccessful()
        {
            return View();
        }
    }
}

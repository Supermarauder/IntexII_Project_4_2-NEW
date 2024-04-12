using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntexII_Project_4_2.Controllers
{
    public class AuthUserController : BaseController
    {

        private readonly IIntexProjectRepository _repo;

        public AuthUserController(IIntexProjectRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Checkout()
        {
            var viewModel = new OrderPrediction
            {
                // Populate Order details as necessary
                Cart = GetCart() // Assuming GetCart retrieves the cart from session or similar
            };

            return View(viewModel);
        }
        [HttpPost]
        [HttpPost]
        public IActionResult Checkout(OrderPrediction viewModel)
        {
            var cart = GetCart();
            var total = cart.CalculateTotal();

            viewModel.Order.Amount = (int?)total; // Set Order.Amount to the cart's total

            // Set additional Order properties (Date, DayOfWeek, Time)
            var currentDateTime = DateTime.Now;
            viewModel.Order.Date = currentDateTime.ToString("MM/dd/yyyy");
            viewModel.Order.DayOfWeek = currentDateTime.ToString("ddd");
            viewModel.Order.Time = currentDateTime.Hour;

            // Add the order using the repository
            _repo.AddOrder(viewModel.Order);

            // Clear the cart after a successful checkout and save it
            cart.Clear();
            SaveCart(cart);

            return RedirectToAction("Confirmation0");
        }


        public IActionResult Confirmation0()
        {
            return View();
        }
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

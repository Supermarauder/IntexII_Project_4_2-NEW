using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IntexII_Project_4_2.Controllers
{
    public class AuthUserController : Controller

    {
        private IIntexProjectRepository _repo;

        private readonly ApplicationDbContext _context;

        private readonly InferenceSession _session;
        public readonly string _onnxModelPath;

        public AuthUserController(ApplicationDbContext context, IHostEnvironment hostEnvironment)
        {
            _context = context;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "model_final.onnx");
            _session = new InferenceSession(_onnxModelPath);

        }
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
        public IActionResult CustomerInfo()
        {
            var viewModel = new OrderPrediction
            {
                Order = new Order(), // Initialize as necessary
                Customer = new Customer() // Initialize as necessary
            };

            return View(viewModel); // Pass the model to the view
        }

        [HttpPost]
        public IActionResult CustomerInfo(OrderPrediction viewModel)
        {
            if (ModelState.IsValid)
            {
                // Set the current date and time
                var currentDateTime = DateTime.Now;

                // Assuming 'Date' is stored as a string. You might need to adjust the format.
                viewModel.Order.Date = currentDateTime.ToString("MM/dd/yyyy");

                // If 'Time' is intended to store hours and minutes, adjust accordingly.
                // This example simply stores the hour for illustration.
                viewModel.Order.Time = currentDateTime.Hour;

                // Set the day of the week
                viewModel.Order.DayOfWeek = currentDateTime.ToString("ddd");

                // Add Order entity to DbSet<Order>
                _context.Orders.Add(viewModel.Order);
                // Add Customer entity to DbSet<Customer>
                _context.Customers.Add(viewModel.Customer);

                // Save changes asynchronously to the database
                _context.SaveChangesAsync();
            }
            return View(viewModel);
        }
    }
}

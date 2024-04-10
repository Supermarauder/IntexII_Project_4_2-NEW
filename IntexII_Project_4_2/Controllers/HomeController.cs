using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IntexII_Project_4_2.Controllers
{
    public class HomeController : Controller
    {
        private IIntexProjectRepository _repo;
        public HomeController(IIntexProjectRepository temp) 
        {
            _repo = temp;
        }

        public IActionResult Index()
        {

            return View();
        }


        public IActionResult About()
        {
            return View();
        }
        public IActionResult CartSummary()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ProductDetail()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult ViewProducts(int pageNum, string[] categories, string[] colors)
        {
            int pageSize = 5;
            pageNum = Math.Max(1, pageNum);

            IQueryable<Product> query = _repo.Products.AsQueryable();

            // Apply category filters if provided
            if (categories != null && categories.Length > 0)
            {
                query = query.Where(p => categories.Any(cat => p.Category.Contains(cat)));
            }

            // Apply color filters if provided
            if (colors != null && colors.Length > 0)
            {
                query = query.Where(p => colors.Contains(p.PrimaryColor));
            }

            int totalItems = query.Count();

            List<Product> filteredProducts = query
                .OrderBy(p => p.Name)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var productList = new ProductListViewModel
            {
                Products = filteredProducts,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(productList);
        }


    }
}

using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Infrastructure;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IntexII_Project_4_2.Controllers
{
    public class HomeController : Controller
    {
        private IIntexProjectRepository _repo;
        private Cart GetCart()
        {
            return HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetJson("cart", cart);
        }

        //public IActionResult RemoveFromCart(int productId, string returnUrl)
        //{
        //    Cart cart = GetCart();
        //    Product product = _repo.Products.FirstOrDefault(p => p.ProductId == productId);

        //    if (product != null)
        //    {
        //        cart.RemoveItem(productId);
        //        SaveCart(cart);
        //    }

        //    return Redirect(returnUrl);  // Assuming returnUrl is a valid path
        //}
        //public HomeController(IIntexProjectRepository temp);
        private InferenceSession _session;
        public string _onnxModelPath;
        public HomeController(IIntexProjectRepository temp, IHostEnvironment hostEnvironment) 
        {
            _repo = temp;

            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "model.onnx");
            _session = new InferenceSession(_onnxModelPath);
        }

        public IActionResult Index()
        {
            var topRecommendationIds = _repo.TopRecommendations.Select(tr => tr.ProductID).ToList();
            var topRecommendations = _repo.Products
                .Where(p => topRecommendationIds.Contains(p.ProductId))
                .ToList();

            var viewModel = new IndexViewModel
            {
                Recommendations = topRecommendations
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            Product product = _repo.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            Cart cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
            cart.AddItem(product, quantity);
            HttpContext.Session.SetJson("cart", cart);

            return RedirectToPage("/Cart");
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
        public IActionResult ProductDetail(int id)
        {
            Product product = _repo.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(); // Or any other error handling
            }

            ItemRecommendation recommendation = _repo.ItemRecommendations.FirstOrDefault(r => r.ProductID == id);

            List<Product> recommendedProducts = new List<Product>();
            if (recommendation != null)
            {
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation1));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation2));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation3));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation4));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation5));
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Recommendations = recommendedProducts
            };

            return View(viewModel);
        }
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ViewProducts(int pageNum, string[] categories, string[] colors, int pageSize = 5)
        {
            pageNum = Math.Max(1, pageNum); // Ensure pageNum is at least 1

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
                    ItemsPerPage = pageSize, // Use the pageSize parameter
                    TotalItems = totalItems
                }
            };

            return View(productList);
        }


    }
}

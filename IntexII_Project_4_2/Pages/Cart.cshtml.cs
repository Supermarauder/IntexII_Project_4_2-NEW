using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Infrastructure;
using IntexII_Project_4_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;

namespace IntexII_Project_4_2.Pages
{
    public class CartModel : PageModel
    {
        private IIntexProjectRepository _repo;
        public CartModel(IIntexProjectRepository temp)
        {
            _repo = temp;
        }
        public Cart? Cart { get; set; }

        public string ReturnURL { get; set; } = "/";
        public void OnGet(string returnUrl)
        {
            ReturnURL = returnUrl ?? "/";
            Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        }
        public IActionResult OnPost(int productId, string returnUrl)
        {
            Product prod = _repo.Products
                .FirstOrDefault(x => x.ProductId == productId);

            if (prod != null)
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(prod, 1);
                HttpContext.Session.SetJson("cart", Cart);
            }

            return RedirectToPage(new { returnUrl = returnUrl });
        }
    }
}

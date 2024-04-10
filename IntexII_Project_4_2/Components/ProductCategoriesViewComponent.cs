using Microsoft.AspNetCore.Mvc;
using IntexII_Project_4_2.Models;

namespace IntexII_Project_4_2.Components
{
    public class ProductCategoriesViewComponent : ViewComponent
    {
        private IIntexProjectRepository _intexRepo;
        
        //Constructor
        public ProductCategoriesViewComponent(IIntexProjectRepository temp) 
        { 
            _intexRepo = temp;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedProductCategory = RouteData?.Values["productCategory"];

            var projectTypes = _intexRepo.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x);

            return View(projectTypes);
        }
    }
}

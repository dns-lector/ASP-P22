using ASP_P22.Data;
using ASP_P22.Models.Shop;
using ASP_P22.Models.User;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ASP_P22.Controllers
{
    public class ShopController(DataContext dataContext, IStorageService storageService) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;

        public IActionResult Index()
        {
            ShopIndexPageModel model = new()
            {
                Categories = [.._dataContext.Categories],
            };
            if (HttpContext.Session.Keys.Contains("productModelErrors"))
            {
                model.Errors = JsonSerializer.Deserialize<Dictionary<String, String>>(
                    HttpContext.Session.GetString("productModelErrors")!
                );
                HttpContext.Session.Remove("productModelErrors");
            }
            return View(model);
        }

        public ViewResult Category([FromRoute] String id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                    .Categories
                    .Include(c => c.Products)
                    .FirstOrDefault(c => c.Slug == id)
            };
            return View(model);
        }

        public ViewResult Product([FromRoute] String id)
        {
            ShopProductPageModel model = new()
            {
                Product = _dataContext
                    .Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Products)              
                    .FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id)
            };
            return View(model);
        }

        public RedirectToActionResult AddProduct([FromForm] ShopProductFormModel formModel)
        {
            Dictionary<String, String> errors = [];
            bool status = true;
            if (formModel == null)
            {
                status = false;
                errors["ModelState"] = "Модель не передано";
            }
            else
            {
                if (String.IsNullOrEmpty(formModel.Name))
                {
                    status = false;
                    errors["Name"] = "Ім'я не може бути порожнім";
                }
                // ...
            }

            if(status)  // Валідація пройдена - додаємо до БД
            {
                // Зберігаємо всі файли
                String? imagesCsv = null;
                if (formModel!.Images != null)
                {
                    imagesCsv = "";
                    foreach (IFormFile file in formModel!.Images)
                    {
                        imagesCsv += _storageService.Save(file) + ',';
                    }
                }
                _dataContext.Products.Add(new Data.Entities.Product
                {
                    Id = Guid.NewGuid(),
                    Name = formModel.Name,
                    CategoryId = formModel.CategoryId,
                    Description = formModel.Description,
                    Slug = formModel.Slug,
                    Price = formModel.Price,
                    Stock = formModel.Stock,
                    ImagesCsv = imagesCsv,
                });
                _dataContext.SaveChanges();
            }
            HttpContext.Session.SetString(
                "productModelErrors",
                JsonSerializer.Serialize(errors)
            );
            return RedirectToAction(nameof(Index));
        }
    }
}
/* Д.З. Реалізувати валідацію форми додавання нового товару.
 * Вивести усі повідомлення про помилки валідації у складі 
 * форми.
 */

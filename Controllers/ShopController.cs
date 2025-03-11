using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models.Shop;
using ASP_P22.Models.User;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Controllers
{
    public class ShopController(DataAccessor dataAccessor, DataContext dataContext, IStorageService storageService) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IStorageService _storageService = storageService;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            // ShopIndexPageModel model = new()
            // {
            //     Categories = [.._dataContext.Categories],
            // };
            ShopIndexPageModel model = _dataAccessor.CategoriesList();
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
                        .ThenInclude(p => p.Rates)
                    .FirstOrDefault(c => c.Slug == id)
            };
            return View(model);
        }

        public ViewResult Product([FromRoute] String id)
        {
            String? authUserId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

            var Product = _dataContext
                    .Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Products)
                    .Include(p => p.Rates)
                        .ThenInclude(r => r.User)
                    .FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id);
            
            ShopProductPageModel model = new()
            {
                Product = Product
            };

            if(Product != null && authUserId != null)
            {
                var cds = _dataContext
                    .CartDetails.Where(cd =>
                        cd.ProductId == Product.Id &&
                        cd.Cart.UserId.ToString() == authUserId );

                model.CanUserRate = cds.Any();

                model.UserRate = 
                    _dataContext.Rates.FirstOrDefault(r =>
                        r.ProductId == Product.Id &&
                        r.UserId.ToString() == authUserId);

                model.AuthUserId = authUserId;
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult Rate([FromBody] ShopRateFormModel formModel)
        {
            Guid userId;
            try { userId = Guid.Parse(formModel.UserId); }
            catch { return Json(new { status = 400, message = "Unparseable UUID 'userId'" }); }
            
            Guid productId;
            try { productId = Guid.Parse(formModel.ProductId); }
            catch { return Json(new { status = 400, message = "Unparseable UUID 'productId'" }); }

            var user = _dataContext.Users.Include(u => u.Rates).FirstOrDefault(u => u.Id == userId);
            if(user == null)
            {
                return Json(new { status = 401, message = $"User '{userId}' unauthorized" });
            }

            var product = _dataContext.Products.FirstOrDefault(p => p.Id == productId);
            if(product == null)
            {
                return Json(new { status = 404, message = $"Product '{productId}' not found" });
            }

            var givenRate = user.Rates?.FirstOrDefault(r => r.ProductId == productId);
            if (givenRate != null)   // оновлення раніше даної оцінки
            {
                givenRate.Comment = formModel.Comment;
                givenRate.Rating = formModel.Rating;
                givenRate.Moment = DateTime.Now;
            }
            else  // нова оцінка
            {
                givenRate = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(formModel.UserId),
                    ProductId = Guid.Parse(formModel.ProductId),
                    Rating = formModel.Rating,
                    Comment = formModel.Comment,
                    Moment = DateTime.Now
                };
                _dataContext.Rates.Add(givenRate);
            }
            _dataContext.SaveChanges();

            return Json(new { status = 200, message = "OK", data = givenRate });
        }

        [HttpPut]
        public JsonResult AddToCart([FromRoute] String id)
        {
            String? userId = HttpContext
                .User
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)
                ?.Value;

            if(userId == null)
            {
                return Json(new { status = 401, message = "UnAuthorized" });
            }
            Guid uid = Guid.Parse(userId);

            // Перевіряємо id на UUID 
            Guid productId;
            try { productId = Guid.Parse(id); }
            catch
            {
                return Json(new { status = 400, message = "UUID required" });
            }
            // Перевіряємо id на належність товару
            var product = _dataContext
                .Products
                .FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { status = 404, message = "Product not found" });
            }

            // Шукаємо відкритий кошик користувача
            var cart = _dataContext
                .Carts
                .FirstOrDefault(
                    c => c.UserId == uid &&
                    c.MomentBuy == null &&
                    c.MomentCancel == null);

            if (cart == null)  // немає відкритого - тоді відкриваємо новий
            {
                cart = new Data.Entities.Cart()
                {
                    Id = Guid.NewGuid(),
                    MomentOpen = DateTime.Now,
                    UserId = uid,
                    Price = 0
                };
                _dataContext.Carts.Add(cart);
            }

            // Перевіряємо чи є такий товар у кошику
            var cd = _dataContext
                .CartDetails
                .FirstOrDefault(d =>
                    d.CartId == cart.Id &&
                    d.ProductId == product.Id
                );
            if (cd != null)  // товар вже є у кошику
            {
                cd.Cnt += 1;
                cd.Price += product.Price;
            }
            else   // товару немає - створюємо новий запис
            {
                cd = new Data.Entities.CartDetail()
                {
                    Id = Guid.NewGuid(),
                    Moment = DateTime.Now,
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Cnt = 1,
                    Price = product.Price
                };
                _dataContext.CartDetails.Add(cd);
            }
            cart.Price += product.Price;
            _dataContext.SaveChanges();
            return Json(new { status = 201, message = "Created" });
        }

        [HttpPatch]
        public JsonResult ModifyCart([FromRoute] String id, [FromQuery] int delta)
        {
            Guid cartDetailId;
            try
            {
                cartDetailId = Guid.Parse(id);
            }
            catch
            {
                return Json(new { status = 400, message = "id unrecognized" });
            }
            if (delta == 0)
            {
                return Json(new { status = 400, message = "dummy action" });
            }
            var cartDetail = _dataContext
                .CartDetails
                .Include(cd => cd.Product)
                .Include(cd => cd.Cart)
                .FirstOrDefault(cd => cd.Id == cartDetailId);

            if (cartDetail is null)
            {
                return Json(new { status = 404, message = "id respond no item" });
            }
            // Д.З. У методі ModifyCart додати перевірку на власність -
            // елемент кошику, що змінюється, належить авторизованому користувачу
            // За відсутності авторизації також надіслати відмову у змінах

            // Перевіряємо delta
            // 1) що її врахування не призведе до від"ємних чисел
            if (cartDetail.Cnt + delta < 0)
            {
                return Json(new { status = 422, message = "decrement too large" });
            }
            // 2) що кількість не перевищує товарні залишки
            if (cartDetail.Cnt + delta > cartDetail.Product.Stock)
            {
                return Json(new { status = 406, message = "increment too large" });
            }

            if(cartDetail.Cnt + delta == 0)  // видалення останнього
            {
                cartDetail.Cart.Price += delta * cartDetail.Product.Price;
                _dataContext.CartDetails.Remove(cartDetail);
            }
            else
            {
                cartDetail.Cnt += delta;
                cartDetail.Price += delta * cartDetail.Product.Price;
                cartDetail.Cart.Price += delta * cartDetail.Product.Price;
            }
            _dataContext.SaveChanges();
            return Json(new { status = 202, message = "Accepted" });
        }

        [HttpDelete]
        public JsonResult CloseCart([FromRoute] String id)
        {
            // Чи користувач авторизований?
            String? sid = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (sid == null)
            {
                return Json(new { status = 401, message = "UnAuthorized" });
            }

            // чи є такий кошик
            Guid cartId;
            try { cartId = Guid.Parse(id); }
            catch { return Json(new { status = 400, message = "id unrecognized" }); }

            var cart = _dataContext
                .Carts
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.Id == cartId);
            if(cart == null)
            {
                return Json(new { status = 404, message = "Requested ID Not Found" });
            }

            // Чи належить він авторизованому користувачеві?           
            if(cart.UserId.ToString() != sid)
            {
                return Json(new { status = 403, message = "Forbidden" });
            }
            String cartAction = Request.Headers["Cart-Action"].ToString();
            if(cartAction == "Buy")
            {
                cart.MomentBuy = DateTime.Now;
                // Скорочуємо кількість товарів
                foreach(var cd in cart.CartDetails)
                {
                    cd.Product.Stock -= cd.Cnt;
                }
            }
            else
            {
                cart.MomentCancel = DateTime.Now;
            }

            _dataContext.SaveChanges();
            return Json(new { status = 200, message = "OK" });
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
/* Д.З. Реалізувати перевірку даних щодо коментаря-оцінки
 * - може не бути оцінки
 * - може не бути коментаря
 * - але не обох одразу
 * - коментар не коротше 5 символів
 * ** дату коментування виводити
 *  = якщо вона є сьогодні, то тільки час, 
 *  = інакше дата
 * ** замінити поле введення оцінки "зірочками" 
 * 
 */
/* API - Application-Program Interface
 * Program - інформаційний "центр", відповідальний за постійне збереження даних
 * Application - застосунок, частіше за все UI, що взаємодіє з "центром" для обміну даними
 *    Застосунок також може зберігати дані, але мова іде про тимчасові дані.
 * у одного Program може бути кілька Applications
 * API - як інтерфейс - правила/форма за якими передаються дані між Program та Applications
 * 
 * Веб-АРІ - різновид АРІ, що базується на протоколі НТТР
 * 
 *    3rd Persons ---- Backend (Program) ------ Test
 *                     / API   |       \ API
 *                 Site      Mobile   Desktop
 *                 
 *                 
 * Створити сторінку REST
 * зазначити на ній обмеження REST, а також порівняння MVC та АРІ контролерів
 * Додати до головної сторінки (Home/Index) посилання на дану сторінку
 * До ДЗ додавати скріншоти результатів роботи сайту
 *           
 */

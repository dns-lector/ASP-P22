using ASP_P22.Models.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Data
{
    public class DataAccessor(DataContext dataContext, IHttpContextAccessor httpContextAccessor)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public ShopIndexPageModel CategoriesList()
        {
            ShopIndexPageModel model = new()
            {
                Categories = [.._dataContext.Categories],
            };
            foreach(var c in model.Categories)
            {
                c.ImagesCsv = c.ImagesCsv == null ? null : String.Join(',',
                    c.ImagesCsv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => StoragePrefix + i)
                );
            }
            return model;
            /* Д.З. Реалізувати клонування сутностей EF при модифікації адрес
             * зображень, що передаються у відповідь від бекенда
             * (метод DataAccessor::CategoriesList)
             */
        }
        public ShopCategoryPageModel CategoryById(String id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                    .Categories
                    .Include(c => c.Products)
                        .ThenInclude(p => p.Rates)
                    .FirstOrDefault(c => c.Slug == id) 
            };
            if (model.Category != null)
            {
                model.Category = model.Category with {
                    Products = model.Category
                    .Products
                    .Select(p => p with { 
                        ImagesCsv = p.ImagesCsv == null
                        ? StoragePrefix + "no-image.jpg"
                        : String.Join(',',
                            p.ImagesCsv
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => StoragePrefix + i)
                        )
                    }).ToList()
                };
            }
            //foreach(var product in model.Category?.Products ?? [])
            //{
            //    product.ImagesCsv = product.ImagesCsv == null 
            //        ? StoragePrefix + "no-image.jpg"
            //        : String.Join(',',
            //            product.ImagesCsv
            //            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            //            .Select(i => StoragePrefix + i)
            //        );
            //}
            _dataContext.SaveChanges();
            return model;
        }

        private String StoragePrefix => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/";
    }
}

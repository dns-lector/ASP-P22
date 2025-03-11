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
                    .Split(',')
                    .Select(i => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/" + i)
                );
            }
            return model;
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
            return model;
        }
    }
}

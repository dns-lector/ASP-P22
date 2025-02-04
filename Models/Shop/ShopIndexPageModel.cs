using ASP_P22.Data.Entities;

namespace ASP_P22.Models.Shop
{
    public class ShopIndexPageModel
    {
        public Category[] Categories { get; set; } = [];
        public Dictionary<String, String>? Errors { get; set; }
    }
}

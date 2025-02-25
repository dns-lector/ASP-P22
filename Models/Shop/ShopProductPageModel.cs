using ASP_P22.Data.Entities;

namespace ASP_P22.Models.Shop
{
    public class ShopProductPageModel
    {
        public Product? Product { get; set; }
        public bool CanUserRate { get; set; }
        public Rate? UserRate { get; set; }
        public String? AuthUserId { get; set; }
    }
}
/* Д.З. Реалізувати відображення "Аналогічних товарів" у вигляді
 * "карток" з зображеннями та цінами тощо.
 * Додати на сторінки товарів та окремого товару у верхню частину
 * рядок зі зменшеними картками усіх категорій товарів.
 */

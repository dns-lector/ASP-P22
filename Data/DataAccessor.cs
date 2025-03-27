using ASP_P22.Controllers;
using ASP_P22.Data.Entities;
using ASP_P22.Models.Shop;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using ASP_P22.Services.Storage;
using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Security.Claims;

namespace ASP_P22.Data
{
    public class DataAccessor(
            DataContext dataContext, 
            IHttpContextAccessor httpContextAccessor,
            IKdfService kdfService,
            IConfiguration configuration)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IConfiguration _configuration = configuration;

        public void AddToCart(String userId, String productId)
        {
            Guid userGuid;
            try { userGuid = Guid.Parse(userId); }
            catch { throw new Exception("User ID is not valid UUID"); }
            Guid productGuid;
            try { productGuid = Guid.Parse(productId); }
            catch { throw new Exception("Product ID is not valid UUID"); }
            // Перевіряємо id на належність товару
            var product = _dataContext
                .Products
                .FirstOrDefault(p => p.Id == productGuid);
            if (product == null)
            {
                throw new Exception("Product ID is not found");
            }

            // Шукаємо відкритий кошик користувача
            var cart = _dataContext
                .Carts
                .FirstOrDefault(
                    c => c.UserId == userGuid &&
                    c.MomentBuy == null &&
                    c.MomentCancel == null);

            if (cart == null)  // немає відкритого - тоді відкриваємо новий
            {
                cart = new Data.Entities.Cart()
                {
                    Id = Guid.NewGuid(),
                    MomentOpen = DateTime.Now,
                    UserId = userGuid,
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
        }

        public Cart? GetCartInfo(String userId, String? cartId)
        {
            Guid userGuid;
            try { userGuid = Guid.Parse(userId); }
            catch { throw new Exception("User ID is not valid UUID"); }
            Cart? cart;
            if (cartId == null)
            {
                cart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefault(c => 
                        c.UserId == userGuid &&
                        c.MomentBuy == null &&
                        c.MomentCancel == null);
            }
            else
            {
                Guid cartGuid;
                try { cartGuid = Guid.Parse(cartId); }
                catch { throw new Exception("Cart ID is not valid UUID"); }
                cart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefault(c => c.Id == cartGuid);
            }
            
            if (cart == null)
            {
                return null;
            }
            if (cart.UserId != userGuid)
            {
                throw new AccessViolationException("Forbidden");
            }
            cart = cart with
            {
                CartDetails = [.. cart.CartDetails.Select(c => c with
                {
                    Product = c.Product with { 
                        ImagesCsv = c.Product.ImagesCsv == null
                        ? StoragePrefix + "no-image.jpg"
                        : String.Join(',',
                            c.Product.ImagesCsv
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => StoragePrefix + i)
                        ) }
                })]
            };
            return cart;
        }

        public void ModifyCart(String id, int delta)
        {
            Guid cartDetailId;
            try
            {
                cartDetailId = Guid.Parse(id);
            }
            catch
            {
                throw new Win32Exception(400, "id unrecognized");
            }
            if (delta == 0)
            {
                throw new Win32Exception(400, "dummy action");
            }
            var cartDetail = _dataContext
                .CartDetails
                .Include(cd => cd.Product)
                .Include(cd => cd.Cart)
                .FirstOrDefault(cd => cd.Id == cartDetailId);

            if (cartDetail is null)
            {
                throw new Win32Exception(404, "id respond no item");
            }
            // Перевіряємо delta
            // 1) що її врахування не призведе до від"ємних чисел
            if (cartDetail.Cnt + delta < 0)
            {
                throw new Win32Exception(422, "decrement too large");
            }
            // 2) що кількість не перевищує товарні залишки
            if (cartDetail.Cnt + delta > cartDetail.Product.Stock)
            {
                throw new Win32Exception(406, "increment too large");
            }

            if (cartDetail.Cnt + delta == 0)  // видалення останнього
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
        }

        public Entities.AuthToken CreateTokenForUserAccess(Entities.UserAccess userAccess)
        {
            int lifetime = _configuration
                .GetSection("AuthToken")
                .GetSection("Lifetime")
                .Get<int>();

            var token = _dataContext
                .AuthTokens
                .FirstOrDefault(t => t.Sub == userAccess.Id && t.Exp > DateTime.Now);

            if (token != null)
            {
                token.Exp = token.Exp.AddSeconds(lifetime);
            }
            else
            {
                token = new Entities.AuthToken
                {
                    Jti = Guid.NewGuid(),
                    Iss = "ASP_P22",
                    Sub = userAccess.Id,
                    Aud = null,
                    Iat = DateTime.Now,
                    Exp = DateTime.Now.AddSeconds(lifetime),
                    Nbf = null,
                };
                _dataContext.AuthTokens.Add(token);
            }
            _dataContext.SaveChanges();
            return token;
        }

        public Entities.UserAccess? BasicAuthenticate()
        {
            String? authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();  // "Basic dGVzdDoxMjM="
            if (String.IsNullOrEmpty(authHeader))
            {
                throw new Exception("Authorization header required");
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                throw new Exception($"Authorization scheme error: '{authScheme}' required");
            }
            String credentials = authHeader[authScheme.Length..];  // "dGVzdDoxMjM="

            String authData = System.Text.Encoding.UTF8.GetString(
                Base64UrlTextEncoder.Decode(credentials));         // "test:123"

            String[] parts = authData.Split(':', 2);               // ["test", "123"]
            if (parts.Length != 2)
            {
                throw new Exception("Authorization credentials malformed");
            }
            // login - parts[0], password - parts[1]
            var ua = _dataContext
                .UsersAccess
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.Login == parts[0]);

            if (ua == null)
            {
                throw new Exception("Authorization rejected");
            }
            var (iter, len) = KdfSettings();
            String dk1 = _kdfService.Dk(parts[1], ua.Salt, iter, len);
            if (dk1 != ua.Dk)
            {
                throw new Exception("Authorization rejected.");
            }
            return ua;
        }
        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }

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

        public ShopProductPageModel ProductById(String id)
        {
            String? authUserId = _httpContextAccessor.HttpContext?.User.Claims
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

            if (Product != null && authUserId != null)
            {
                var cds = _dataContext
                    .CartDetails.Where(cd =>
                        cd.ProductId == Product.Id &&
                        cd.Cart.UserId.ToString() == authUserId);

                model.CanUserRate = cds.Any();

                model.UserRate =
                    _dataContext.Rates.FirstOrDefault(r =>
                        r.ProductId == Product.Id &&
                        r.UserId.ToString() == authUserId);

                model.AuthUserId = authUserId;
            }
            return model;
        }

        private String StoragePrefix => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/";
    }
}

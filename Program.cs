using ASP_P22.Data;
using ASP_P22.Middleware.Auth;
using ASP_P22.Services.Hash;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using ASP_P22.Services.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .WithOrigins("http://localhost:3000")
            .WithHeaders("Authorization");
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
// реєструємо власні служби у контейнері builder.Services
// наступну інструкцію можна розуміти як "будуть запитувати IRandomService -
//  повернути об'єкт класу GuidRandomService"
//builder.Services.AddSingleton<IRandomService, GuidRandomService>();
builder.Services.AddSingleton<IRandomService, AbcRandomService>();
builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IKdfService, PbKdf1Service>();
builder.Services.AddSingleton<IStorageService, LocalStorageService>();

String connectionString = builder.Configuration.GetConnectionString("LocalMS")!;
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString)
);
builder.Services.AddScoped<DataAccessor>();


// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseSession();

app.UseAuthSession();
// app.UseAuthToken();
app.UseJwtToken();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using ASP_P22.Data;
using ASP_P22.Services.Hash;
using ASP_P22.Services.Random;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// �������� ������ ������ � ��������� builder.Services
// �������� ���������� ����� ������� �� "������ ���������� IRandomService -
//  ��������� ��'��� ����� GuidRandomService"
//builder.Services.AddSingleton<IRandomService, GuidRandomService>();
builder.Services.AddSingleton<IRandomService, AbcRandomService>();
builder.Services.AddSingleton<IHashService, Md5HashService>();

String connectionString = builder.Configuration.GetConnectionString("LocalMS")!;
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString)
);

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.AspNetCore.Authentication.Cookies;
using NuvoxSound.Data;
using NuvoxSound.Business;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Registramos tu capa de datos
builder.Services.AddScoped<NuvoxSound.Data.ProductoData>();
builder.Services.AddScoped<NuvoxSound.Data.VentaData>();
// Registramos el servicio de la API de Watson
builder.Services.AddScoped<NuvoxSound.Business.WatsonService>();
// Registramos las capas del Dashboard
builder.Services.AddScoped<DashboardData>();
builder.Services.AddScoped<DashboardBusiness>();
//Activar el manejo de sesiones en memoria para el carrito de compras
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Evita que el cookie sea accesible desde JavaScript
    options.Cookie.IsEssential = true; // Asegura que el cookie se envíe incluso si el usuario no ha dado su consentimiento 
});
//Registramos la capa de negocio
builder.Services.AddScoped<NuvoxSound.Business.UsuarioBusiness>();
//Configurar el guardian de autenticación
builder.Services.AddAuthentication("NuvoxCookie")
    .AddCookie("NuvoxCookie", options =>
    {
        options.Cookie.Name = "UserLoginCookie"; // Nombre de la cookie de autenticación
        options.LoginPath = "/Account/Login"; // Ruta a la página de inicio de sesión
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta a la página de acceso denegado
        options.ExpireTimeSpan= TimeSpan.FromHours(2); // Tiempo de expiración de la cookie
    });
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
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

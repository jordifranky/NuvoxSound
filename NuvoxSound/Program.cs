using Microsoft.AspNetCore.Authentication.Cookies;
using NuvoxSound.Data;
using NuvoxSound.Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Permite que el HTML (Layout) pueda leer la memoria de la sesión para el contador del carrito
builder.Services.AddHttpContextAccessor();

// ========================================================
// REGISTRO DE CAPA DE DATOS (SQL)
// ========================================================
builder.Services.AddScoped<NuvoxSound.Data.ProductoData>();
builder.Services.AddScoped<NuvoxSound.Data.VentaData>();
builder.Services.AddScoped<NuvoxSound.Data.UsuarioData>();


builder.Services.AddScoped<NuvoxSound.Data.Data>();

// ========================================================
// REGISTRO DE CAPA DE NEGOCIO Y SERVICIOS
// ========================================================
builder.Services.AddScoped<NuvoxSound.Business.WatsonService>();
builder.Services.AddScoped<DashboardBusiness>();
builder.Services.AddScoped<NuvoxSound.Business.UsuarioBusiness>();

// Activar el manejo de sesiones en memoria para el carrito de compras
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Evita que el cookie sea accesible desde JavaScript
    options.Cookie.IsEssential = true; // Asegura que el cookie se envíe incluso si el usuario no ha dado su consentimiento 
});

// Configurar el guardian de autenticación
builder.Services.AddAuthentication("NuvoxCookie")
    .AddCookie("NuvoxCookie", options =>
    {
        options.Cookie.Name = "UserLoginCookie"; // Nombre de la cookie de autenticación
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tiempo de expiración de la cookie
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
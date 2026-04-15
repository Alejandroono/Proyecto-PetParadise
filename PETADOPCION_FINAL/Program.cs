using Microsoft.EntityFrameworkCore;
using PETADOPCION_FINAL.Models;
using PETADOPCION_FINAL.Services;

var builder = WebApplication.CreateBuilder(args);

// Controladores y vistas
builder.Services.AddControllersWithViews();

// Configuración de sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Solo SqlServer en producción
builder.Services.AddDbContext<PetadopcionContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("servidor"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    );
});
// Servicio de email
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error500");
app.UseStatusCodePagesWithReExecute("/Home/Error404/{0}");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.Run();

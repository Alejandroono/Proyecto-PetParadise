using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PETADOPCION_FINAL.Controllers;
using PETADOPCION_FINAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using BCryptNet = BCrypt.Net.BCrypt;

public class PasswordValidator
{
    private PetadopcionContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PetadopcionContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new PetadopcionContext(options);

        // ✅ Genera el hash real de BCrypt para que el controller pueda compararlo
        string passwordPlana = "A12344567#";
        string hashBcrypt = BCryptNet.HashPassword(passwordPlana);

        context.Usuarios.Add(new Usuario
        {
            IdUsuario = 1,
            IdRol = 1,
            Nombre = "Admin",
            Apellido = "Test",
            Email = "admin@test.com",
            PasswordHash = hashBcrypt,          // ← hash real, no texto plano
            Telefono = "3001234567",
            Direccion = "Calle 123 #45-67",
            Ciudad = "Bogota",
            FechaRegistro = DateTime.Now,
            Estado = true,
        });

        context.SaveChanges();
        return context;
    }

    private HomeController GetController(PetadopcionContext context)
    {
        var logger = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(logger.Object, context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new FakeSession();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    [Fact]
    public async Task Login_ContrasenaCorrecta_Redirecciona()
    {
        var context = GetContext();
        var controller = GetController(context);
        var resultado = await controller.Index("admin@test.com", "A12344567#"); // ← igual al PasswordHash
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Login_ContrasenaIncorrecta_RetornaVistaConError()
    {
        var context = GetContext();
        var controller = GetController(context);
        var resultado = await controller.Index("admin@test.com", "wrong");
        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Login_ContrasenaVacia_RetornaVista()
    {
        var context = GetContext();
        var controller = GetController(context);
        var resultado = await controller.Index("admin@test.com", "");
        Assert.IsType<ViewResult>(resultado);
    }
}

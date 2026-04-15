using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetParadise.Controllers;   // AJUSTA si tu namespace es diferente
using PetParadise.Data;          // AJUSTA si tu namespace es diferente
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class SolicitudesAdopcionesControllerTests
{
    // 🔹 Crear base de datos en memoria
    private PetadopcionContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<PetparadiseContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

        return new PetadopcionContext(options);
    }

    // 🔹 Crear configuración falsa
    private IConfiguration GetConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Clave", "Valor"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Index_SinSesion_RedireccionaAHome()
    {
        // Arrange
        var context = GetDbContext();
        var configuration = GetConfiguration();

        var controller = new SolicitudesAdopcionesController(context, configuration);

        // Simular HttpContext SIN sesión
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new DummySession();

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.Index();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}
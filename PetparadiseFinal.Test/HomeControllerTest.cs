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
using Microsoft.AspNetCore.Components.Forms;

public class HomeControllerTest
{
    private PetadopcionContext GetContext()
    {
        var options = new DbContextOptionsBuilder<PetadopcionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PetadopcionContext(options);
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
    public async Task SinSesionRedirecciona()
    {
        var context = GetContext();
        var controller = GetController(context);

        var result = await controller.Principal();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.NotNull(redirect);
    }
}
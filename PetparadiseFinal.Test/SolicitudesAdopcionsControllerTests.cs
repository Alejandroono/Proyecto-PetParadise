using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PETADOPCION_FINAL.Controllers;
using PETADOPCION_FINAL.Models;
using PETADOPCION_FINAL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace PetParadise.Test
{
    public class SolicitudesAdopcionsControllerTests
    {
        // =====================================================
        // HELPERS
        // =====================================================
        private PetadopcionContext GetContext()
        {
            var options = new DbContextOptionsBuilder<PetadopcionContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new PetadopcionContext(options);

            context.Roles.Add(new Role { IdRol = 1, NombreRol = "Usuario" });

            context.Usuarios.Add(new Usuario
            {
                IdUsuario = 1,
                Nombre = "Juan",
                Apellido = "Perez",
                Email = "juan@test.com",
                PasswordHash = "1234",
                Ciudad = "Montería",
                Direccion = "Calle 123",
                Telefono = "3001234567",
                Estado = true,
                FechaRegistro = DateTime.Now,
                IdRol = 1
            });

            context.Mascotas.Add(new Mascota
            {
                IdMascota = 1,
                Nombre = "Firulais",
                Tipo = "Perro",
                Sexo = "Macho",
                Edad = 2,
                IdUsuarioPublicador = 1,
                EstadoAdopcion = "Disponible",

                
                Descripcion = "Perro amigable y juguetón",
                EstadoSalud = "Saludable",
                Raza = "Criollo",
                Ubicacion = "Montería"
            });

            context.SolicitudesAdopcions.Add(new SolicitudesAdopcion
            {
                IdSolicitud = 1,
                IdMascota = 1,
                IdUsuarioSolicitante = 1,
                Estado = "En revisión",
                Mensaje = "Quiero adoptar",
                FechaSolicitud = DateTime.Now
            });

            context.SaveChanges();
            return context;
        }

        private IConfiguration GetConfig()
        {
            var settings = new Dictionary<string, string>
            {
                { "TestKey", "TestValue" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        private Mock<IEmailService> GetEmailMock()
        {
            return new Mock<IEmailService>();
        }

        // =====================================================
        // TESTS
        // =====================================================

        [Fact]
        public async Task Index_RetornaVistaConModelo()
        {
            var controller = new SolicitudesAdopcionsController(GetContext(), GetConfig(), GetEmailMock().Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Details_IdNull_RetornaNotFound()
        {
            var controller = new SolicitudesAdopcionsController(GetContext(), GetConfig(), GetEmailMock().Object);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_Existente_RetornaVista()
        {
            var controller = new SolicitudesAdopcionsController(GetContext(), GetConfig(), GetEmailMock().Object);

            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model);
        }

        [Fact]
        public async Task Details_NoExiste_RetornaNotFound()
        {
            var controller = new SolicitudesAdopcionsController(GetContext(), GetConfig(), GetEmailMock().Object);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Eliminar_EliminaSolicitudCorrectamente()
        {
            var context = GetContext();
            var controller = new SolicitudesAdopcionsController(context, GetConfig(), GetEmailMock().Object);

            var result = await controller.Eliminar(1);

            Assert.IsType<JsonResult>(result);
            Assert.Empty(context.SolicitudesAdopcions);
        }

        [Fact]
        public async Task Eliminar_NoExiste_RetornaError()
        {
            var context = GetContext();
            var controller = new SolicitudesAdopcionsController(context, GetConfig(), GetEmailMock().Object);

            var result = await controller.Eliminar(999);

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task ProcesarSolicitud_Aceptar_CambiaEstadoYMas()
        {
            var context = GetContext();
            var emailMock = GetEmailMock();
            var controller = new SolicitudesAdopcionsController(context, GetConfig(), emailMock.Object);

            var result = await controller.ProcesarSolicitud(1, "Aceptar");

            Assert.IsType<JsonResult>(result);

            var solicitud = context.SolicitudesAdopcions.First(s => s.IdSolicitud == 1);
            var mascota = context.Mascotas.First(m => m.IdMascota == 1);

            Assert.Equal("Aprobada", solicitud.Estado);
            Assert.Equal("En Proceso", mascota.EstadoAdopcion);
            emailMock.Verify(e => e.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcesarSolicitud_Rechazar_CambiaEstado()
        {
            var context = GetContext();
            var emailMock = GetEmailMock();
            var controller = new SolicitudesAdopcionsController(context, GetConfig(), emailMock.Object);

            var result = await controller.ProcesarSolicitud(1, "Rechazar");

            var solicitud = context.SolicitudesAdopcions.First(s => s.IdSolicitud == 1);
            var mascota = context.Mascotas.First(m => m.IdMascota == 1);

            Assert.Equal("Rechazada", solicitud.Estado);
            Assert.Equal("Disponible", mascota.EstadoAdopcion);
            emailMock.Verify(e => e.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcesarSolicitud_Restablecer_NoEnviaCorreo()
        {
            var context = GetContext();
            var emailMock = GetEmailMock();
            var controller = new SolicitudesAdopcionsController(context, GetConfig(), emailMock.Object);

            var result = await controller.ProcesarSolicitud(1, "Restablecer");

            var solicitud = context.SolicitudesAdopcions.First(s => s.IdSolicitud == 1);
            var mascota = context.Mascotas.First(m => m.IdMascota == 1);

            Assert.Equal("En revisión", solicitud.Estado);
            Assert.Equal("Disponible", mascota.EstadoAdopcion);
            emailMock.Verify(e => e.Enviar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcesarSolicitud_NoExiste_RetornaError()
        {
            var controller = new SolicitudesAdopcionsController(GetContext(), GetConfig(), GetEmailMock().Object);

            var result = await controller.ProcesarSolicitud(999, "Aceptar");

            Assert.IsType<JsonResult>(result);
        }

    }
}
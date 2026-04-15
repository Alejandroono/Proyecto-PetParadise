using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PETADOPCION_FINAL.Controllers;
using PETADOPCION_FINAL.Models;

namespace PETADOPCION_FINAL.Test
{
    public class SolicitudesAdopcionesControllerTests
    {
        private IConfiguration GetConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "DataSource=:memory:"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private PetadopcionContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PetadopcionContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            return new PetadopcionContext(options);
        }

        [Fact]
        public void CrearSolicitudAdopcion_DeberiaGuardarEnBD()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new SolicitudesAdopcionesController(context);

            var solicitud = new SolicitudAdopcion
            {
                NombreSolicitante = "Beatriz",
                MascotaId = 1,
                Estado = "Pendiente"
            };

            // Act
            controller.Create(solicitud);

            // Assert
            var guardado = context.SolicitudesAdopciones.FirstOrDefault(s => s.NombreSolicitante == "Beatriz");
            Assert.NotNull(guardado);
            Assert.Equal("Pendiente", guardado.Estado);
        }
    }
}

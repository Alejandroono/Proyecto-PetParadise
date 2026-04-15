using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PETADOPCION_FINAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_rol = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__6ABCB5E07F8076EA", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_rol = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ciudad = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__4E3E04AD09D5D372", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK__Usuarios__id_rol__3C69FB99",
                        column: x => x.id_rol,
                        principalTable: "Roles",
                        principalColumn: "id_rol");
                });

            migrationBuilder.CreateTable(
                name: "Documentos_Usuario",
                columns: table => new
                {
                    id_documento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    UrlRecibo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    url_archivo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    UrlFormulario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado_verificacion = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true, defaultValue: "Pendiente"),
                    fecha_subida = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__5D2EE7E5454B2962", x => x.id_documento);
                    table.ForeignKey(
                        name: "FK__Documento__id_us__4F7CD00D",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    id_mascota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario_publicador = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    edad = table.Column<int>(type: "int", nullable: true),
                    tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    raza = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    sexo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    estado_salud = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    ubicacion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    estado_adopcion = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true, defaultValue: "Disponible"),
                    fecha_publicacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Mascotas__6F037352D0E195EE", x => x.id_mascota);
                    table.ForeignKey(
                        name: "FK__Mascotas__id_usu__412EB0B6",
                        column: x => x.id_usuario_publicador,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "Noticias",
                columns: table => new
                {
                    IdNoticia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnlaceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdUsuarioPublicador = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticias", x => x.IdNoticia);
                    table.ForeignKey(
                        name: "FK_Noticias_Usuarios_IdUsuarioPublicador",
                        column: x => x.IdUsuarioPublicador,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adopciones",
                columns: table => new
                {
                    id_adopcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    id_adoptante = table.Column<int>(type: "int", nullable: false),
                    fecha_adopcion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Adopcion__E4E156E909F1875A", x => x.id_adopcion);
                    table.ForeignKey(
                        name: "FK__Adopcione__id_ad__5441852A",
                        column: x => x.id_adoptante,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK__Adopcione__id_ma__534D60F1",
                        column: x => x.id_mascota,
                        principalTable: "Mascotas",
                        principalColumn: "id_mascota");
                });

            migrationBuilder.CreateTable(
                name: "Mascota_Fotos",
                columns: table => new
                {
                    id_foto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    url_foto = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    es_principal = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Mascota___620EA3A513BE8CD2", x => x.id_foto);
                    table.ForeignKey(
                        name: "FK__Mascota_F__id_ma__44FF419A",
                        column: x => x.id_mascota,
                        principalTable: "Mascotas",
                        principalColumn: "id_mascota");
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes_Adopcion",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    id_usuario_solicitante = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true, defaultValue: "En revisión"),
                    mensaje = table.Column<string>(type: "text", nullable: true),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solicitu__5C0C31F34F79777A", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK__Solicitud__id_ma__49C3F6B7",
                        column: x => x.id_mascota,
                        principalTable: "Mascotas",
                        principalColumn: "id_mascota");
                    table.ForeignKey(
                        name: "FK__Solicitud__id_us__4AB81AF0",
                        column: x => x.id_usuario_solicitante,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adopciones_id_adoptante",
                table: "Adopciones",
                column: "id_adoptante");

            migrationBuilder.CreateIndex(
                name: "IX_Adopciones_id_mascota",
                table: "Adopciones",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Usuario_id_usuario",
                table: "Documentos_Usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_Fotos_id_mascota",
                table: "Mascota_Fotos",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_id_usuario_publicador",
                table: "Mascotas",
                column: "id_usuario_publicador");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_IdUsuarioPublicador",
                table: "Noticias",
                column: "IdUsuarioPublicador");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Adopcion_id_mascota",
                table: "Solicitudes_Adopcion",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Adopcion_id_usuario_solicitante",
                table: "Solicitudes_Adopcion",
                column: "id_usuario_solicitante");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_id_rol",
                table: "Usuarios",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__AB6E61643ED7EC1F",
                table: "Usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adopciones");

            migrationBuilder.DropTable(
                name: "Documentos_Usuario");

            migrationBuilder.DropTable(
                name: "Mascota_Fotos");

            migrationBuilder.DropTable(
                name: "Noticias");

            migrationBuilder.DropTable(
                name: "Solicitudes_Adopcion");

            migrationBuilder.DropTable(
                name: "Mascotas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

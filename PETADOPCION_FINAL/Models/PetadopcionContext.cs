using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PETADOPCION_FINAL.Models;

public partial class PetadopcionContext : DbContext
{
    public PetadopcionContext(DbContextOptions<PetadopcionContext> options)
      : base(options)
    {
    }

    public virtual DbSet<Adopcione> Adopciones { get; set; }

    public virtual DbSet<DocumentosUsuario> DocumentosUsuarios { get; set; }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<MascotaFoto> MascotaFotos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SolicitudesAdopcion> SolicitudesAdopcions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Noticia> Noticias { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=tcp:petadopcion.database.windows.net,1433;Initial Catalog=PETADOPCION;Persist Security Info=False;User ID=jsalgadobolano17;Password=Gabriel12#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adopcione>(entity =>
        {
            entity.HasKey(e => e.IdAdopcion).HasName("PK__Adopcion__E4E156E909F1875A");

            entity.Property(e => e.IdAdopcion).HasColumnName("id_adopcion");
            entity.Property(e => e.FechaAdopcion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_adopcion");
            entity.Property(e => e.IdAdoptante).HasColumnName("id_adoptante");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");

            entity.HasOne(d => d.IdAdoptanteNavigation).WithMany(p => p.Adopciones)
                .HasForeignKey(d => d.IdAdoptante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Adopcione__id_ad__5441852A");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.Adopciones)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Adopcione__id_ma__534D60F1");
        });

        modelBuilder.Entity<DocumentosUsuario>(entity =>
        {
            entity.HasKey(e => e.IdDocumento).HasName("PK__Document__5D2EE7E5454B2962");

            entity.ToTable("Documentos_Usuario");

            entity.Property(e => e.IdDocumento).HasColumnName("id_documento");
            entity.Property(e => e.EstadoVerificacion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado_verificacion");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.UrlRecibo)
                .HasMaxLength(50)
                .IsUnicode(false)
               .HasColumnName("UrlRecibo");
            entity.Property(e => e.UrlArchivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url_archivo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.DocumentosUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Documento__id_us__4F7CD00D");
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.IdMascota).HasName("PK__Mascotas__6F037352D0E195EE");

            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.EstadoAdopcion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Disponible")
                .HasColumnName("estado_adopcion");
            entity.Property(e => e.EstadoSalud)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("estado_salud");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_publicacion");
            entity.Property(e => e.IdUsuarioPublicador).HasColumnName("id_usuario_publicador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Raza)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("raza");
            entity.Property(e => e.Sexo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("sexo");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("ubicacion");

            entity.HasOne(d => d.IdUsuarioPublicadorNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdUsuarioPublicador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mascotas__id_usu__412EB0B6");
        });

        modelBuilder.Entity<MascotaFoto>(entity =>
        {
            entity.HasKey(e => e.IdFoto).HasName("PK__Mascota___620EA3A513BE8CD2");

            entity.ToTable("Mascota_Fotos");

            entity.Property(e => e.IdFoto).HasColumnName("id_foto");
            entity.Property(e => e.EsPrincipal)
                .HasDefaultValue(false)
                .HasColumnName("es_principal");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.UrlFoto)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url_foto");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.MascotaFotos)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mascota_F__id_ma__44FF419A");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__6ABCB5E07F8076EA");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<SolicitudesAdopcion>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PK__Solicitu__5C0C31F34F79777A");

            entity.ToTable("Solicitudes_Adopcion");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("En revisión")
                .HasColumnName("estado");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdUsuarioSolicitante).HasColumnName("id_usuario_solicitante");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solicitud__id_ma__49C3F6B7");

            entity.HasOne(d => d.IdUsuarioSolicitanteNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdUsuarioSolicitante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solicitud__id_us__4AB81AF0");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD09D5D372");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__AB6E61643ED7EC1F").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ciudad");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__id_rol__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

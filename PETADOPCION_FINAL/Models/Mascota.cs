using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace PETADOPCION_FINAL.Models;

public partial class Mascota
{
    public int IdMascota { get; set; }

    public int IdUsuarioPublicador { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras (sin números ni símbolos).")]
    public string Nombre { get; set; } = null!;
  public int? Edad { get; set; }
    [Required(ErrorMessage = "El tipo de mascota es obligatorio (ej. Perro, Gato).")]
    public string? Tipo { get; set; }
    [Required(ErrorMessage = "La raza es obligatoria.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Mínimo 2 letras para la raza.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "La raza no puede llevar números.")]
    public string? Raza { get; set; }
    [Required(ErrorMessage = "El sexo de la mascota es obligatorio.")]
    public string? Sexo { get; set; }
    [Required(ErrorMessage = "El estado de salud es obligatorio.")]
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La descripción debe tener entre 4 y 200 caracteres.")]
    public string? EstadoSalud { get; set; }
    [Required(ErrorMessage = "La descripción es fundamental para el proceso de adopción.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres.")]
    public string? Descripcion { get; set; }
    [Required(ErrorMessage = "La ubicación es obligatoria para que los adoptantes te encuentren.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "La ubicación debe tener entre 3 y 80 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s,#-]+$", ErrorMessage = "La ubicación contiene caracteres no permitidos.")]
    public string? Ubicacion { get; set; }

    public string? EstadoAdopcion { get; set; }

    public DateTime? FechaPublicacion { get; set; }

    public virtual ICollection<Adopcione> Adopciones { get; set; } = new List<Adopcione>();

    public virtual Usuario IdUsuarioPublicadorNavigation { get; set; } = null!;
    [Required(ErrorMessage = "Debe subir al menos una foto de la mascota.")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos una imagen.")]
    public virtual ICollection<MascotaFoto> MascotaFotos { get; set; } = new List<MascotaFoto>();

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();
}

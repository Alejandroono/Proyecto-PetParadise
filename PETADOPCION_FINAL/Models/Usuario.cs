using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PETADOPCION_FINAL.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public string Nombre { get; set; } = null!;
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener al menos 3 caracteres.")]
    public string Apellido { get; set; } = null!;
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage = "El correo debe tener un formato válido.")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
  ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, incluyendo mayúsculas, minúsculas, números y un carácter especial.")]
    public string PasswordHash { get; set; } = null!;
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener exactamente 10 dígitos.")]
    public string? Telefono { get; set; }
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(int.MaxValue, MinimumLength = 5, ErrorMessage = "La dirección debe tener mínimo 5 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9\s\.,#\-]+$",
       ErrorMessage = "La dirección solo puede contener letras, números, espacios y los caracteres ., #-")]
    public string? Direccion { get; set; }
    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "La ciudad debe tener mínimo 2 caracteres.")]
    public string? Ciudad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Adopcione> Adopciones { get; set; } = new List<Adopcione>();

    public virtual ICollection<DocumentosUsuario> DocumentosUsuarios { get; set; } = new List<DocumentosUsuario>();

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();
}

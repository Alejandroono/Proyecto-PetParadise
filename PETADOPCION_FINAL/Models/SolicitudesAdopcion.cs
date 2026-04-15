using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace PETADOPCION_FINAL.Models;

public partial class SolicitudesAdopcion
{
    public int IdSolicitud { get; set; }

    public int IdMascota { get; set; }

    public int IdUsuarioSolicitante { get; set; }

    public string? Estado { get; set; }
    public string? Mensaje { get; set; }

    public DateTime? FechaSolicitud { get; set; }
   
    public virtual Mascota IdMascotaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioSolicitanteNavigation { get; set; } = null!;
}

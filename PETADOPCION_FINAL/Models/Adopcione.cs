using System;
using System.Collections.Generic;

namespace PETADOPCION_FINAL.Models;

public partial class Adopcione
{
    public int IdAdopcion { get; set; }

    public int IdMascota { get; set; }

    public int IdAdoptante { get; set; }

    public DateTime? FechaAdopcion { get; set; }

    public virtual Usuario IdAdoptanteNavigation { get; set; } = null!;

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;
}

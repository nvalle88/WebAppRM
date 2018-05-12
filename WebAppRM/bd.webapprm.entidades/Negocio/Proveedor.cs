namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
    public partial class Proveedor
    {
        public Proveedor()
        {
            Factura = new HashSet<Factura>();
            RecepcionActivoFijo = new HashSet<RecepcionActivoFijo>();
            RecepcionArticulos = new HashSet<RecepcionArticulos>();
        }

        [Key]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Nombre:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe introducir los {0}")]
        [Display(Name = "Apellidos:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Los {0} no pueden tener m�s de {1} y menos de {2}")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Identificaci�n:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "La {0} no puede tener m�s de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "La {0} solo puede contener n�meros.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Direcci�n:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "C�digo:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Codigo { get; set; }

        [Display(Name = "L�nea de servicio:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string LineaServicio { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Raz�n social:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener m�s de {1} y menos de {2}")]
        public string RazonSocial { get; set; }

        [Display(Name = "Tel�fono:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Telefono { get; set; }

        [Display(Name = "Correo electr�nico:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        [EmailAddress(ErrorMessage = "El {0} es inv�lido")]
        public string Email { get; set; }

        [Display(Name = "Cargo:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Cargo { get; set; }

        [Display(Name = "Observaciones:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Observaciones { get; set; }

        [Display(Name = "Emisi�n:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Emision { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "�Activo?")]
        public bool Activo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        public virtual ICollection<Factura> Factura { get; set; }

        public virtual ICollection<RecepcionActivoFijo> RecepcionActivoFijo { get; set; }

        public virtual ICollection<RecepcionArticulos> RecepcionArticulos { get; set; }
    }
}

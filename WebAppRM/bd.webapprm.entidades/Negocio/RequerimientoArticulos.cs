using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class RequerimientoArticulos
    {
        public RequerimientoArticulos()
        {
            RequerimientosArticulosDetalles = new HashSet<RequerimientosArticulosDetalles>();
            SalidaArticulos = new HashSet<SalidaArticulos>();
        }

        [Key]
        public int IdRequerimientoArticulos { get; set; }

        [Display(Name = "Funcionario que solicita:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdFuncionarioSolicitante { get; set; }
        public virtual Empleado FuncionarioSolicitante { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de solicitud:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaSolicitud { get; set; }

        [Display(Name = "Fecha de aprobado o denegado:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaAprobadoDenegado { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }
        
        [Display(Name = "Observaciones:")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Las {0} no pueden tener más de {1} y menos de {2}")]
        public string Observaciones { get; set; }

        public virtual ICollection<RequerimientosArticulosDetalles> RequerimientosArticulosDetalles { get; set; }
        public virtual ICollection<SalidaArticulos> SalidaArticulos { get; set; }
    }
}

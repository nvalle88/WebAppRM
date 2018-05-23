using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class RecepcionActivoFijoDetalleDatosEspecificosViewModel
    {
        public int IdRecepcionActivoFijoDetalle { get; set; }

        [Display(Name = "Serie:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "La {0} solo puede contener números.")]
        public string Serie { get; set; }

        [Display(Name = "Número de chasis:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroChasis { get; set; }

        [Display(Name = "Número de motor:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroMotor { get; set; }

        [Display(Name = "Placa:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "La {0} solo puede contener números.")]
        public string Placa { get; set; }

        [Display(Name = "Número de clave catastral:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroClaveCatastral { get; set; }

        [Display(Name = "Empleado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        [Display(Name = "Bodega:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int? IdBodega { get; set; }
        public virtual Bodega Bodega { get; set; }

        public bool IsBodega { get; set; }

        public List<PropiedadValor> Validar()
        {
            List<PropiedadValor> errores = new List<PropiedadValor>();
            if (IsBodega)
            {
                if (IdBodega == null)
                    errores.Add(new PropiedadValor { Propiedad = "IdBodega", Valor = "Tiene que seleccionar una Bodega." });
            }
            else
            {
                if (IdEmpleado == null)
                    errores.Add(new PropiedadValor { Propiedad = "IdEmpleado", Valor = "Tiene que seleccionar un Empleado." });
            }
            return errores;
        }
    }

    public class PropiedadValor
    {
        public string Propiedad { get; set; }
        public string Valor { get; set; }
    }
}

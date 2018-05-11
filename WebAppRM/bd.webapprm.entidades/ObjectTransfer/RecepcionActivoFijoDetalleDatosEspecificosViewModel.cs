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
        public string Serie { get; set; }

        [Display(Name = "Número de chasis:")]
        public string NumeroChasis { get; set; }

        [Display(Name = "Número de motor:")]
        public string NumeroMotor { get; set; }

        [Display(Name = "Placa:")]
        public string Placa { get; set; }

        [Display(Name = "Número de clave catastral:")]
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

            if (Serie != null)
            {
                if (Serie.Length < 2 || Serie.Length > 50)
                    errores.Add(new PropiedadValor { Propiedad = "Serie", Valor = "La Serie: no puede tener más de 50 y menos de 2." });
            }

            if (NumeroChasis != null)
            {
                if (NumeroChasis.Length < 2 || NumeroChasis.Length > 50)
                    errores.Add(new PropiedadValor { Propiedad = "NumeroChasis", Valor = "El Número de chasis: no puede tener más de 50 y menos de 2." });
            }

            if (NumeroMotor != null)
            {
                if (NumeroMotor.Length < 2 || NumeroMotor.Length > 50)
                    errores.Add(new PropiedadValor { Propiedad = "NumeroMotor", Valor = "El Número de motor: no puede tener más de 50 y menos de 2." });
            }

            if (Placa != null)
            {
                if (Placa.Length < 2 || Placa.Length > 50)
                    errores.Add(new PropiedadValor { Propiedad = "Placa", Valor = "La Placa: no puede tener más de 50 y menos de 2." });
            }

            if (NumeroClaveCatastral != null)
            {
                if (NumeroClaveCatastral.Length < 2 || NumeroClaveCatastral.Length > 50)
                    errores.Add(new PropiedadValor { Propiedad = "NumeroClaveCatastral", Valor = "El Número de clave catastral: no puede tener más de 50 y menos de 2." });
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

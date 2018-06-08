using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MantenimientoActivoFijo : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var mantenimientoActivoFijo = (MantenimientoActivoFijo)validationContext.ObjectInstance;
            if (mantenimientoActivoFijo.FechaDesde > mantenimientoActivoFijo.FechaHasta)
                yield return new ValidationResult($"La Fecha de inicio no puede ser mayor que la Fecha de fin", new[] { "FechaDesde" });
            yield return ValidationResult.Success;
        }
    }

    public partial class CompaniaSeguro : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var companiaSeguro = (CompaniaSeguro)validationContext.ObjectInstance;
            if (companiaSeguro.FechaInicioVigencia > companiaSeguro.FechaFinVigencia)
                yield return new ValidationResult($"La Fecha de inicio de vigencia no puede ser mayor que la Fecha de fin de vigencia", new[] { "FechaInicioVigencia" });
            yield return ValidationResult.Success;
        }
    }

    public partial class MovilizacionActivoFijo : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var movilizacionActivoFijo = (MovilizacionActivoFijo)validationContext.ObjectInstance;
            if (movilizacionActivoFijo.FechaSalida > movilizacionActivoFijo.FechaRetorno)
                yield return new ValidationResult($"La Fecha de salida no puede ser mayor que la Fecha de retorno", new[] { "FechaSalida" });
            yield return ValidationResult.Success;
        }
    }
}

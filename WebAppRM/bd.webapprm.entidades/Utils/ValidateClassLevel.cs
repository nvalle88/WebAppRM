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
                yield return new ValidationResult($"La fecha de inicio no puede ser mayor que la Fecha de fin", new[] { "FechaDesde" });
            yield return ValidationResult.Success;
        }
    }
}

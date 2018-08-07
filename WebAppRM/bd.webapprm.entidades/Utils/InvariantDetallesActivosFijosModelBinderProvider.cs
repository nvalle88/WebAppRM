using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bd.webapprm.entidades.Utils
{
    public class InvariantDetallesActivosFijosModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(ListadoDetallesActivosFijosViewModel))
                return new InvariantDetallesActivosFijosModelBinder(context.Metadata.ModelType);

            return null;
        }
    }

    public class InvariantDetallesActivosFijosModelBinder : IModelBinder
    {
        private readonly SimpleTypeModelBinder _baseBinder;

        public InvariantDetallesActivosFijosModelBinder(Type modelType)
        {
            _baseBinder = new SimpleTypeModelBinder(modelType);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var result = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSeleccion"),
                IsConfiguracionSmartForm: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSmartForm"),
                IsConfiguracionDetallesRecepcion: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionDetallesRecepcion"),
                IsConfiguracionSeleccionComponentes: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSeleccionComponentes"),
                IsConfiguracionListadoComponentes: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoComponentes"),
                IsConfiguracionListadoMantenimientos: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoMantenimientos"),
                IsConfiguracionListadoProcesosJudiciales: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoProcesosJudiciales"),
                IsConfiguracionListadoRevalorizaciones: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoRevalorizaciones"),
                IsConfiguracionSeleccionAltas: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSeleccionAltas"),
                IsConfiguracionListadoAltasGestionar: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoAltasGestionar"),
                IsConfiguracionListadoAltas: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoAltas"),
                IsConfiguracionSeleccionBajas: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSeleccionBajas"),
                IsConfiguracionListadoBajasGestionar: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoBajasGestionar"),
                IsConfiguracionListadoBajas: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoBajas"),
                IsConfiguracionListadoMovilizaciones: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionListadoMovilizaciones"),
                IsConfiguracionSeleccionDisabled: ObtenerIsConfiguracion(bindingContext, "IsConfiguracionSeleccionDisabled"));
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        private bool ObtenerIsConfiguracion(ModelBindingContext bindingContext, string requestForm)
        {
            bool isConfiguracion;
            bool.TryParse(bindingContext.HttpContext.Request.Form[$"arrConfiguraciones[{requestForm}]"], out isConfiguracion);
            return isConfiguracion;
        }
    }
}

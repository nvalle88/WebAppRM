using bd.log.guardar.Inicializar;
using bd.webapprm.entidades.Utils;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.servicios.Servicios;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace bd.webapprm.web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(config => {
                config.ModelBinderProviders.Insert(0, new InvariantDatetimeModelBinderProvider());
                //config.ModelBinderProviders.Insert(1, new InvariantDecimalModelBinderProvider());
                config.ModelBindingMessageProvider.ValueMustBeANumberAccessor = (value) => $"El valor del campo {value} es inválido.";
                config.ModelBindingMessageProvider.ValueMustNotBeNullAccessor = value => $"Debe introducir el {value}";
            });
            services.AddSingleton<IApiServicio, ApiServicio>();

            WebApp.BaseAddressWebAppLogin = Configuration.GetSection("HostWebAppLogin").Value;
            WebApp.NombreAplicacion = Configuration.GetSection("NombreAplicacion").Value;

            var HostSeguridad = Configuration.GetSection("HostServicioSeguridad").Value;
            WebApp.BaseAddressRM = Configuration.GetSection("HostServiciosRecursosMateriales").Value;
            WebApp.BaseAddressSeguridad = Configuration.GetSection("HostServicioSeguridad").Value;
            WebApp.BaseAddressTH = Configuration.GetSection("HostServiciosTalentoHumano").Value;
            AppGuardarLog.BaseAddress = Configuration.GetSection("HostServicioLog").Value;

            services.AddMemoryCache();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var defaultCulture = new CultureInfo("en-us");
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultCulture),
                SupportedCultures = new List<CultureInfo> { defaultCulture },
                SupportedUICultures = new List<CultureInfo> { defaultCulture },
                FallBackToParentCultures = false,
                FallBackToParentUICultures = false,
                RequestCultureProviders = new List<IRequestCultureProvider> { }
            });

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //serviceScope.ServiceProvider.GetService<LogDbContext>()
                    //         .Database.Migrate();

                   // serviceScope.ServiceProvider.GetService<InicializacionServico>().InicializacionAsync();
                }
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{id2?}");
            });
        }
    }
}

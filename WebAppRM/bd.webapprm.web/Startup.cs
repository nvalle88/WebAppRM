using bd.log.guardar.Inicializar;
using bd.webapprm.entidades.Utils;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.servicios.Servicios;
using bd.webapprm.web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config => {
                config.ModelBinderProviders.Insert(0, new InvariantDatetimeModelBinderProvider());
                //config.ModelBinderProviders.Insert(1, new InvariantDecimalModelBinderProvider());
                config.ModelBindingMessageProvider.ValueMustBeANumberAccessor = (value) => $"El valor del campo {value} es inválido.";
                config.ModelBindingMessageProvider.ValueMustNotBeNullAccessor = value => $"Debe introducir el {value}";
            });

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddDataProtection().UseCryptographicAlgorithms(new AuthenticatedEncryptionSettings()
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            });

            services.AddDataProtection().SetDefaultKeyLifetime(TimeSpan.FromDays(Convert.ToInt32(Configuration.GetSection("DiasValidosClaveEncriptada").Value)));

            services.AddSingleton<IApiServicio, ApiServicio>();
            services.AddSingleton<IMenuServicio, MenuServicio>();
            services.AddSingleton<IClaimsTransfer, ClaimsTransferServicio>();

            services.AddSingleton<IAuthorizationHandler, RolesHandler>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc().Services.AddAuthorization(options =>
            {
                options.AddPolicy("EstaAutorizado", policy => policy.Requirements.Add(new RolesRequirement()));
            });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(Filtro));
            });

            Func<string, string> AsignarSlash = appsettingsOpcion =>
            {
                if (appsettingsOpcion[appsettingsOpcion.Length - 1] != '/')
                    appsettingsOpcion += "/";

                return appsettingsOpcion;
            };
            WebApp.BaseAddressWebAppLogin = AsignarSlash(Configuration.GetSection("HostWebAppLogin").Value);
            WebApp.NombreAplicacion = Configuration.GetSection("NombreAplicacion").Value;
            WebApp.BaseAddressRM = AsignarSlash(Configuration.GetSection("HostServiciosRecursosMateriales").Value);
            WebApp.BaseAddressSeguridad = AsignarSlash(Configuration.GetSection("HostServicioSeguridad").Value);
            WebApp.BaseAddressTH = AsignarSlash(Configuration.GetSection("HostServiciosTalentoHumano").Value);
            AppGuardarLog.BaseAddress = AsignarSlash(Configuration.GetSection("HostServicioLog").Value);
            WebApp.NivelesMenu = Convert.ToInt32(Configuration.GetSection("NivelMenu").Value);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var defaultCulture = new CultureInfo("es-ec");
            defaultCulture.NumberFormat.NumberDecimalSeparator = ".";
            defaultCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultCulture),
                SupportedCultures = new List<CultureInfo> { defaultCulture },
                SupportedUICultures = new List<CultureInfo> { defaultCulture },
                FallBackToParentCultures = false,
                FallBackToParentUICultures = false,
                RequestCultureProviders = new List<IRequestCultureProvider> { }
            });
            app.UseExceptionHandler("/Home/Error");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", env.EnvironmentName)
                //.WriteTo.RollingFile("log-{Date}.txt")
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            loggerFactory.AddSerilog(logger);
            Log.Logger = logger;
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //serviceScope.ServiceProvider.GetService<LogDbContext>().Database.Migrate();
                    //serviceScope.ServiceProvider.GetService<InicializacionServico>().InicializacionAsync();
                }
            }
            else
            {

            }

            app.UseStaticFiles();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies",
                LoginPath = new PathString("/"),
                AccessDeniedPath = new PathString("/"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                CookieName = "ASPTest",
                ExpireTimeSpan = new TimeSpan(1, 0, 0),
                DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(@"c:\shared-auth-ticket-keys\"))
            });
            //app.UseIdentity();

            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Login}/{id?}/{id2?}");
            });
        }
    }
}

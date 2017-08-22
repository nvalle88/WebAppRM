 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
   public class NacionalidadServicio: INacionalidadServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public NacionalidadServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Nacionalidad nacionalidad)
        {
            try
            {
                var respuesta = Existe(nacionalidad);
                if (!respuesta.IsSuccess)
                {
                    nacionalidad.Nombre = nacionalidad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(nacionalidad);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Ok",
                    };
                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "Existe un nacionalidad con igual nombre...",
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public Response Editar(Nacionalidad nacionalidad)
        {
            try
            {
                var respuesta = Existe(nacionalidad);
                if (!respuesta.IsSuccess)
                {
                    var respuestaNacionalidad = (Nacionalidad)respuesta.Resultado;
                    respuestaNacionalidad.Nombre = nacionalidad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaNacionalidad);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Ok",
                    };

                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "Existe un nacionalidad con igual nombre...",
                    };
                }
            }

            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public Response Eliminar(int idNacionalidad)
        {
            try
            {
                var respuestaNacionalidad = db.Nacionalidad.Find(idNacionalidad);
                if (respuestaNacionalidad != null)
                {
                    db.Remove(respuestaNacionalidad);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Ok",
                    };
                }
                return new Response
                {
                    IsSuccess = false,
                    Message = "No se encontró el nacionalidad",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El nacionalidad no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Nacionalidad nacionalidad)
        {
            var respuestaNacionalidad = db.Nacionalidad.Where(p => p.Nombre.ToUpper() == nacionalidad.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaNacionalidad != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe nacionalidad de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe nacionalidad...",
                Resultado = db.Nacionalidad.Where(p => p.IdNacionalidad == nacionalidad.IdNacionalidad).FirstOrDefault(),
            };
        }


        public Nacionalidad ObtenerNacionalidad(int idNacionalidad)
        {
            return db.Nacionalidad.Where(c => c.IdNacionalidad == idNacionalidad).FirstOrDefault();
        }

        public List<Nacionalidad> ObtenerNacionalidades()
        {
            return db.Nacionalidad.OrderBy(p => p.Nombre).ToList();
        }
        #endregion
    }
}

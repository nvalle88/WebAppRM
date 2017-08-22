 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
   public class TipoIdentificacionServicio: ITipoIdentificacionServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public TipoIdentificacionServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoIdentificacion tipoIdentificacion)
        {
            try
            {
                var respuesta = Existe(tipoIdentificacion);
                if (!respuesta.IsSuccess)
                {
                    tipoIdentificacion.Nombre = tipoIdentificacion.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoIdentificacion);
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
                        Message = "Existe un tipo de identificación con igual nombre...",
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

        public Response Editar(TipoIdentificacion tipoIdentificacion)
        {
            try
            {
                var respuesta = Existe(tipoIdentificacion);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoIdentificacion = (TipoIdentificacion)respuesta.Resultado;
                    respuestaTipoIdentificacion.Nombre = tipoIdentificacion.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoIdentificacion);
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
                        Message = "Existe un tipoIdentificacion con igual nombre...",
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
                var respuestaTipoIdentificacion = db.TipoIdentificacion.Find(idNacionalidad);
                if (respuestaTipoIdentificacion != null)
                {
                    db.Remove(respuestaTipoIdentificacion);
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
                    Message = "No se encontró el tipoIdentificacion",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipoIdentificacion no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoIdentificacion tipoIdentificacion)
        {
            var respuestaTipoIdentificacion = db.TipoIdentificacion.Where(p => p.Nombre.ToUpper() == tipoIdentificacion.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoIdentificacion != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe tipoIdentificacion de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipoIdentificacion...",
                Resultado = db.TipoIdentificacion.Where(p => p.IdTipoIdentificacion == tipoIdentificacion.IdTipoIdentificacion).FirstOrDefault(),
            };
        }


        public TipoIdentificacion ObtenerTipoIdentificacion(int idTipoIdentificacion)
        {
            return db.TipoIdentificacion.Where(c => c.IdTipoIdentificacion == idTipoIdentificacion).FirstOrDefault();
        }

        public List<TipoIdentificacion> ObtenerTiposIdentificaciones()
        {
            return db.TipoIdentificacion.OrderBy(p => p.Nombre).ToList();
        }



       
        #endregion
    }
}

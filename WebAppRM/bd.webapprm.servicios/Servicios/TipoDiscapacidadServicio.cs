 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoDiscapacidadServicio : ITipoDiscapacidadServicio

    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public TipoDiscapacidadServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoDiscapacidad tipoDiscapacidad)
        {
            try
            {
                var respuesta = Existe(tipoDiscapacidad);
                if (!respuesta.IsSuccess)
                {
                    tipoDiscapacidad.Nombre = tipoDiscapacidad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoDiscapacidad);
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
                        Message = "Existe un tipo de discapacidad con igual nombre...",
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

        public Response Editar(TipoDiscapacidad tipoDiscapacidad)
        {
            try
            {
                var respuesta = Existe(tipoDiscapacidad);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoDiscapacidad = (TipoDiscapacidad)respuesta.Resultado;
                    respuestaTipoDiscapacidad.Nombre = tipoDiscapacidad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoDiscapacidad);
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
                        Message = "Existe un tipo de discapacidad con igual nombre...",
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

        public Response Eliminar(int idTipoDiscapacidad)
        {
            try
            {
                var respuestaTipoDiscapacidad = db.TipoDiscapacidad.Find(idTipoDiscapacidad);
                if (respuestaTipoDiscapacidad != null)
                {
                    db.Remove(respuestaTipoDiscapacidad);
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
                    Message = "No se encontró el tipo de discapacidad",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de discapacidad no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoDiscapacidad tipoDiscapacidad)
        {
            var respuestaPais = db.TipoDiscapacidad.Where(p => p.Nombre.ToUpper() == tipoDiscapacidad.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaPais != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe tipo de discapacidad de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe país...",
                Resultado = db.TipoDiscapacidad.Where(p => p.IdTipoDiscapacidad == tipoDiscapacidad.IdTipoDiscapacidad).FirstOrDefault(),
            };
        }

        public TipoDiscapacidad ObtenerTipoDiscapacidad(int idTipoDiscapacidad)
        {
            return db.TipoDiscapacidad.Where(c => c.IdTipoDiscapacidad == idTipoDiscapacidad).FirstOrDefault();
        }

        public List<TipoDiscapacidad> ObtenerTiposDiscapacidades()
        {
            return db.TipoDiscapacidad.OrderBy(p => p.Nombre).ToList();
        }

      
        #endregion
    }
}

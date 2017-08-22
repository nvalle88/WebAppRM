 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoActivoFijoServicio: ITipoActivoFijoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoActivoFijoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoActivoFijo tipoActivoFijo)
        {
            try
            {
                var respuesta = Existe(tipoActivoFijo);
                if (!respuesta.IsSuccess)
                {
                    tipoActivoFijo.Nombre = tipoActivoFijo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoActivoFijo);
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
                        Message = "Existe un tipo de activo fijo con igual nombre...",
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

        public Response Editar(TipoActivoFijo tipoActivoFijo)
        {
            try
            {
                var respuesta = Existe(tipoActivoFijo);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoActivoFijo = (TipoActivoFijo)respuesta.Resultado;
                    respuestaTipoActivoFijo.Nombre = tipoActivoFijo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoActivoFijo);
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
                        Message = "Existe un tipo de activo fijo con igual nombre...",
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

        public Response Eliminar(int idTipoActivoFijo)
        {
            try
            {
                var respuestaTipoActivoFijo = db.TipoActivoFijo.Find(idTipoActivoFijo);
                if (respuestaTipoActivoFijo != null)
                {
                    db.Remove(respuestaTipoActivoFijo);
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
                    Message = "No se encontró el tipo de activo fijo",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de activo fijo no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoActivoFijo tipoActivoFijo)
        {
            var respuestaTipoActivoFijo = db.TipoActivoFijo.Where(p => p.Nombre.ToUpper() == tipoActivoFijo.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoActivoFijo != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de activo fijo de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de activo fijo...",
                Resultado = db.TipoActivoFijo.Where(p => p.IdTipoActivoFijo == tipoActivoFijo.IdTipoActivoFijo).FirstOrDefault(),
            };
        }

        public TipoActivoFijo ObtenerTipoActivoFijo(int idTipoActivoFijo)
        {
            return db.TipoActivoFijo.Where(c => c.IdTipoActivoFijo == idTipoActivoFijo).FirstOrDefault();
        }

        public List<TipoActivoFijo> ObtenerTiposActivosFijos()
        {
            return db.TipoActivoFijo.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

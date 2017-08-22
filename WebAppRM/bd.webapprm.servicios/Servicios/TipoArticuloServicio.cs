 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoArticuloServicio: ITipoArticuloServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoArticuloServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoArticulo tipoArticulo)
        {
            try
            {
                var respuesta = Existe(tipoArticulo);
                if (!respuesta.IsSuccess)
                {
                    tipoArticulo.Nombre = tipoArticulo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoArticulo);
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
                        Message = "Existe un tipo de artículo con igual nombre...",
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

        public Response Editar(TipoArticulo tipoArticulo)
        {
            try
            {
                var respuesta = Existe(tipoArticulo);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoArticulo = (TipoArticulo)respuesta.Resultado;
                    respuestaTipoArticulo.Nombre = tipoArticulo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoArticulo);
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
                        Message = "Existe un tipo de artículo con igual nombre...",
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

        public Response Eliminar(int idTipoArticulo)
        {
            try
            {
                var respuestaTipoArticulo = db.TipoArticulo.Find(idTipoArticulo);
                if (respuestaTipoArticulo != null)
                {
                    db.Remove(respuestaTipoArticulo);
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
                    Message = "No se encontró el tipo de artículo",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de artículo no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoArticulo tipoArticulo)
        {
            var respuestaTipoArticulo = db.TipoArticulo.Where(p => p.Nombre.ToUpper() == tipoArticulo.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoArticulo != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de artículo de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de artículo...",
                Resultado = db.TipoArticulo.Where(p => p.IdTipoArticulo == tipoArticulo.IdTipoArticulo).FirstOrDefault(),
            };
        }

        public TipoArticulo ObtenerTipoArticulo(int idTipoArticulo)
        {
            return db.TipoArticulo.Where(c => c.IdTipoArticulo == idTipoArticulo).FirstOrDefault();
        }

        public List<TipoArticulo> ObtenerTiposArticulos()
        {
            return db.TipoArticulo.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

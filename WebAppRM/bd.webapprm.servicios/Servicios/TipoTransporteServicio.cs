 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoTransporteServicio: ITipoTransporteServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoTransporteServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoTransporte tipoTransporte)
        {
            try
            {
                var respuesta = Existe(tipoTransporte);
                if (!respuesta.IsSuccess)
                {
                    tipoTransporte.Descripcion = tipoTransporte.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoTransporte);
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
                        Message = "Existe un tipo de transporte con igual nombre...",
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

        public Response Editar(TipoTransporte tipoTransporte)
        {
            try
            {
                var respuesta = Existe(tipoTransporte);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoTransporte = (TipoTransporte)respuesta.Resultado;
                    respuestaTipoTransporte.Descripcion = tipoTransporte.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoTransporte);
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
                        Message = "Existe un tipo de transporte con igual nombre...",
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

        public Response Eliminar(int idTipoTransporte)
        {
            try
            {
                var respuestaTipoTransporte = db.TipoTransporte.Find(idTipoTransporte);
                if (respuestaTipoTransporte != null)
                {
                    db.Remove(respuestaTipoTransporte);
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
                    Message = "No se encontró el tipo de transporte",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de transporte no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoTransporte tipoTransporte)
        {
            var respuestaTipoTransporte = db.TipoTransporte.Where(p => p.Descripcion.ToUpper() == tipoTransporte.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoTransporte != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de transporte de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de transporte...",
                Resultado = db.TipoTransporte.Where(p => p.IdTipoTransporte == tipoTransporte.IdTipoTransporte).FirstOrDefault(),
            };
        }

        public TipoTransporte ObtenerTipoTransporte(int idTipoTransporte)
        {
            return db.TipoTransporte.Where(c => c.IdTipoTransporte == idTipoTransporte).FirstOrDefault();
        }

        public List<TipoTransporte> ObtenerTiposTransportes()
        {
            return db.TipoTransporte.OrderBy(p => p.Descripcion).ToList();
        }

        #endregion
    }
}

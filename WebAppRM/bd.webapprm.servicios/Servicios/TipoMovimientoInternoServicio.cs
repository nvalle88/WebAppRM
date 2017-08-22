 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoMovimientoInternoServicio: ITipoMovimientoInternoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoMovimientoInternoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoMovimientoInterno tipoMovimientoInterno)
        {
            try
            {
                var respuesta = Existe(tipoMovimientoInterno);
                if (!respuesta.IsSuccess)
                {
                    tipoMovimientoInterno.Nombre = tipoMovimientoInterno.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoMovimientoInterno);
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
                        Message = "Existe un tipo de movimiento interno con igual nombre...",
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

        public Response Editar(TipoMovimientoInterno tipoMovimientoInterno)
        {
            try
            {
                var respuesta = Existe(tipoMovimientoInterno);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoMovimientoInterno = (TipoMovimientoInterno)respuesta.Resultado;
                    respuestaTipoMovimientoInterno.Nombre = tipoMovimientoInterno.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoMovimientoInterno);
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
                        Message = "Existe un tipo de movimiento interno con igual nombre...",
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

        public Response Eliminar(int idTipoMovimientoInterno)
        {
            try
            {
                var respuestaTipoMovimientoInterno = db.TipoMovimientoInterno.Find(idTipoMovimientoInterno);
                if (respuestaTipoMovimientoInterno != null)
                {
                    db.Remove(respuestaTipoMovimientoInterno);
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
                    Message = "No se encontró el tipo de movimiento interno",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de movimiento interno no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoMovimientoInterno tipoMovimientoInterno)
        {
            var respuestaTipoMovimientoInterno = db.TipoMovimientoInterno.Where(p => p.Nombre.ToUpper() == tipoMovimientoInterno.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoMovimientoInterno != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de movimiento interno de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de movimiento interno...",
                Resultado = db.TipoMovimientoInterno.Where(p => p.IdTipoMovimientoInterno == tipoMovimientoInterno.IdTipoMovimientoInterno).FirstOrDefault(),
            };
        }

        public TipoMovimientoInterno ObtenerTipoMovimientoInterno(int idTipoMovimientoInterno)
        {
            return db.TipoMovimientoInterno.Where(c => c.IdTipoMovimientoInterno == idTipoMovimientoInterno).FirstOrDefault();
        }

        public List<TipoMovimientoInterno> ObtenerTiposMovimientosInternos()
        {
            return db.TipoMovimientoInterno.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

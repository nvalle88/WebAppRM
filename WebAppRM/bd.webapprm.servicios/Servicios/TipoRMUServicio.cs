 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoRMUServicio: ITipoRMUServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoRMUServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoRMU tipoRMU)
        {
            try
            {
                var respuesta = Existe(tipoRMU);
                if (!respuesta.IsSuccess)
                {
                    tipoRMU.Descripcion = tipoRMU.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoRMU);
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
                        Message = "Existe un tipo de RMU con igual nombre...",
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

        public Response Editar(TipoRMU tipoRMU)
        {
            try
            {
                var respuesta = Existe(tipoRMU);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoRMU = (TipoRMU)respuesta.Resultado;
                    respuestaTipoRMU.Descripcion = tipoRMU.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoRMU);
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
                        Message = "Existe un tipo de RMU con igual nombre...",
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

        public Response Eliminar(int idTipoRMU)
        {
            try
            {
                var respuestaTipoRMU = db.TipoRMU.Find(idTipoRMU);
                if (respuestaTipoRMU != null)
                {
                    db.Remove(respuestaTipoRMU);
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
                    Message = "No se encontró el tipo de RMU",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de RMU no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoRMU tipoRMU)
        {
            var respuestaTipoRMU = db.TipoRMU.Where(p => p.Descripcion.ToUpper() == tipoRMU.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoRMU != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de RMU de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de RMU...",
                Resultado = db.TipoRMU.Where(p => p.IdTipoRMU == tipoRMU.IdTipoRMU).FirstOrDefault(),
            };
        }

        public TipoRMU ObtenerTipoRMU(int idTipoRMU)
        {
            return db.TipoRMU.Where(c => c.IdTipoRMU == idTipoRMU).FirstOrDefault();
        }

        public List<TipoRMU> ObtenerTiposRMU()
        {
            return db.TipoRMU.OrderBy(p => p.Descripcion).ToList();
        }

        #endregion
    }
}

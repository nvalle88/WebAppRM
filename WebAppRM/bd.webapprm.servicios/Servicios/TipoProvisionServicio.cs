 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoProvisionServicio: ITipoProvisionServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoProvisionServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoProvision tipoProvision)
        {
            try
            {
                var respuesta = Existe(tipoProvision);
                if (!respuesta.IsSuccess)
                {
                    tipoProvision.Descripcion = tipoProvision.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoProvision);
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
                        Message = "Existe un tipo de provisión con igual nombre...",
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

        public Response Editar(TipoProvision tipoProvision)
        {
            try
            {
                var respuesta = Existe(tipoProvision);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoProvison = (TipoProvision)respuesta.Resultado;
                    respuestaTipoProvison.Descripcion = tipoProvision.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoProvison);
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
                        Message = "Existe un tipo de provision con igual nombre...",
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

        public Response Eliminar(int idTipoProvision)
        {
            try
            {
                var respuestaTipoProvison = db.TipoProvision.Find(idTipoProvision);
                if (respuestaTipoProvison != null)
                {
                    db.Remove(respuestaTipoProvison);
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
                    Message = "No se encontró el tipo de provisión",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de provisión no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoProvision tipoProvision)
        {
            var respuestaTipoProvision = db.TipoProvision.Where(p => p.Descripcion.ToUpper() == tipoProvision.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoProvision != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de provisión de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipo de provisión...",
                Resultado = db.TipoProvision.Where(p => p.IdTipoProvision == tipoProvision.IdTipoProvision).FirstOrDefault(),
            };
        }

        public TipoProvision ObtenerTipoProvision(int idTipoProvision)
        {
            return db.TipoProvision.Where(c => c.IdTipoProvision == idTipoProvision).FirstOrDefault();
        }

        public List<TipoProvision> ObtenerTiposProvisiones()
        {
            return db.TipoProvision.OrderBy(p => p.Descripcion).ToList();
        }

        #endregion
    }
}

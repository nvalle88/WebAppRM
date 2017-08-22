 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoPermisoServicio: ITipoPermisoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoPermisoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoPermiso tipoPermiso)
        {
            try
            {
                var respuesta = Existe(tipoPermiso);
                if (!respuesta.IsSuccess)
                {
                    tipoPermiso.Nombre = tipoPermiso.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoPermiso);
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
                        Message = "Existe un tipo de permiso con igual nombre...",
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

        public Response Editar(TipoPermiso tipoPermiso)
        {
            try
            {
                var respuesta = Existe(tipoPermiso);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoPermiso = (TipoPermiso)respuesta.Resultado;
                    respuestaTipoPermiso.Nombre = tipoPermiso.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoPermiso);
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
                        Message = "Existe un tipo de permiso con igual nombre...",
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

        public Response Eliminar(int idTipoPermiso)
        {
            try
            {
                var respuestaTipoPermiso = db.TipoPermiso.Find(idTipoPermiso);
                if (respuestaTipoPermiso != null)
                {
                    db.Remove(respuestaTipoPermiso);
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
                    Message = "No se encontró el tipo de permiso",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de permiso no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoPermiso tipoPermiso)
        {
            var respuestaTipoPermiso = db.TipoPermiso.Where(p => p.Nombre.ToUpper() == tipoPermiso.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoPermiso != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de permiso de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de permiso...",
                Resultado = db.TipoPermiso.Where(p => p.IdTipoPermiso == tipoPermiso.IdTipoPermiso).FirstOrDefault(),
            };
        }

        public TipoPermiso ObtenerTipoPermiso(int idTipoPermiso)
        {
            return db.TipoPermiso.Where(c => c.IdTipoPermiso == idTipoPermiso).FirstOrDefault();
        }

        public List<TipoPermiso> ObtenerTiposPermisos()
        {
            return db.TipoPermiso.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

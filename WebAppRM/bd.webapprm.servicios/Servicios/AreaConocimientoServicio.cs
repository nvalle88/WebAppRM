 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
  public class AreaConocimientoServicio: IAreaConocimientoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public AreaConocimientoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(AreaConocimiento areaConocimiento)
        {
            try
            {
                var respuesta = Existe(areaConocimiento);
                if (!respuesta.IsSuccess)
                {
                    areaConocimiento.Descripcion = areaConocimiento.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(areaConocimiento);
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
                        Message = "Existe un areaConocimiento con igual nombre...",
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

        public Response Editar(AreaConocimiento areaConocimiento)
        {
            try
            {
                var respuesta = Existe(areaConocimiento);
                if (!respuesta.IsSuccess)
                {
                    var respuestaNivelDesarrollo = (AreaConocimiento)respuesta.Resultado;
                    respuestaNivelDesarrollo.Descripcion = areaConocimiento.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaNivelDesarrollo);
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
                        Message = "Existe un areaConocimiento con igual nombre...",
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

        public Response Eliminar(int idNivelDesarrollo)
        {
            try
            {
                var respuestaNivelDesarrollo = db.AreaConocimiento.Find(idNivelDesarrollo);
                if (respuestaNivelDesarrollo != null)
                {
                    db.Remove(respuestaNivelDesarrollo);
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
                    Message = "No se encontró el areaConocimiento",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El areaConocimiento no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(AreaConocimiento areaConocimiento)
        {
            var respuestaNivelDesarrollo = db.AreaConocimiento.Where(p => p.Descripcion.ToUpper() == areaConocimiento.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaNivelDesarrollo != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe areaConocimiento de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe areaConocimiento...",
                Resultado = db.AreaConocimiento.Where(p => p.IdAreaConocimiento == areaConocimiento.IdAreaConocimiento).FirstOrDefault(),
            };
        }


        public AreaConocimiento ObtenerAreaConocimiento(int idNivelDesarrollo)
        {
            return db.AreaConocimiento.Where(c => c.IdAreaConocimiento == idNivelDesarrollo).FirstOrDefault();
        }

        public List<AreaConocimiento> ObtenerAreasConocimientos()
        {
            return db.AreaConocimiento.OrderBy(p => p.Descripcion).ToList();
        }
        #endregion
    }
}

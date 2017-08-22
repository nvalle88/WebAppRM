 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
  public class TipoSangreServicio: ITipoSangreServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoSangreServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoSangre tipoSangre)
        {
            try
            {
                var respuesta = Existe(tipoSangre);
                if (!respuesta.IsSuccess)
                {
                    tipoSangre.Nombre = tipoSangre.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoSangre);
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
                        Message = "Existe un tipo de sangre con igual nombre...",
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

        public Response Editar(TipoSangre tipoSangre)
        {
            try
            {
                var respuesta = Existe(tipoSangre);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoSangre = (TipoSangre)respuesta.Resultado;
                    respuestaTipoSangre.Nombre = tipoSangre.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoSangre);
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
                        Message = "Existe un tipo de sangre con igual nombre...",
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

        public Response Eliminar(int idTipoSangre)
        {
            try
            {
                var respuestaTipoSangre = db.TipoSangre.Find(idTipoSangre);
                if (respuestaTipoSangre != null)
                {
                    db.Remove(respuestaTipoSangre);
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
                    Message = "No se encontró el tipo de sangre",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de sangre no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoSangre tipoSangre)
        {
            var respuestaTipoSangre = db.TipoSangre.Where(p => p.Nombre.ToUpper() == tipoSangre.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoSangre != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe tipo de sangre de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipo de sangre...",
                Resultado = db.TipoSangre.Where(p => p.IdTipoSangre == tipoSangre.IdTipoSangre).FirstOrDefault(),
            };
        }

        public TipoSangre ObtenerTipoSangre(int idTipoSangre)
        {
            return db.TipoSangre.Where(c => c.IdTipoSangre == idTipoSangre).FirstOrDefault();
        }

        public List<TipoSangre> ObtenerTiposSangres()
        {
            return db.TipoSangre.OrderBy(p => p.Nombre).ToList();
        }

  
        #endregion
    }
}

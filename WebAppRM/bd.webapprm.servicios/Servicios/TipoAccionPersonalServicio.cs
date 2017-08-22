 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoAccionPersonalServicio: ITipoAccionPersonalServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoAccionPersonalServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoAccionPersonal tipoAccionPersonal)
        {
            try
            {
                var respuesta = Existe(tipoAccionPersonal);
                if (!respuesta.IsSuccess)
                {
                    tipoAccionPersonal.Descripcion = tipoAccionPersonal.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoAccionPersonal);
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
                        Message = "Existe un tipo de acción de personal con igual nombre...",
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

        public Response Editar(TipoAccionPersonal tipoAccionPersonal)
        {
            try
            {
                var respuesta = Existe(tipoAccionPersonal);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoAccionPersonal = (TipoAccionPersonal)respuesta.Resultado;
                    respuestaTipoAccionPersonal.Descripcion = tipoAccionPersonal.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoAccionPersonal);
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
                        Message = "Existe un tipo de acción de personal con igual nombre...",
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

        public Response Eliminar(int idTipoAccionPersonal)
        {
            try
            {
                var respuestaTipoAccionPersonal = db.TipoAccionPersonal.Find(idTipoAccionPersonal);
                if (respuestaTipoAccionPersonal != null)
                {
                    db.Remove(respuestaTipoAccionPersonal);
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
                    Message = "No se encontró el tipo de acción de personal",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de acción de personal no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoAccionPersonal tipoAccionPersonal)
        {
            var respuestaTipoAccionPersonal = db.TipoAccionPersonal.Where(p => p.Descripcion.ToUpper() == tipoAccionPersonal.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoAccionPersonal != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de acción de personal de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de acción de personal...",
                Resultado = db.TipoAccionPersonal.Where(p => p.IdTipoAccionPersonal == tipoAccionPersonal.IdTipoAccionPersonal).FirstOrDefault(),
            };
        }

        public TipoAccionPersonal ObtenerTipoAccionPersonal(int idTipoAccionPersonal)
        {
            return db.TipoAccionPersonal.Where(c => c.IdTipoAccionPersonal == idTipoAccionPersonal).FirstOrDefault();
        }

        public List<TipoAccionPersonal> ObtenerTiposAccionesPersonal()
        {
            return db.TipoAccionPersonal.OrderBy(p => p.Descripcion).ToList();
        }

        #endregion
    }
}

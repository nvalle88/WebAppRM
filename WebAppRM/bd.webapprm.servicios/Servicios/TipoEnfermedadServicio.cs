 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bd.webappcompartido.servicios.Servicios
{
  public  class TipoEnfermedadServicio: ITipoEnfermedadServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoEnfermedadServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoEnfermedad tipoEnfermedad)
        {
            try
            {
                var respuesta = Existe(tipoEnfermedad);
                if (!respuesta.IsSuccess)
                {
                    tipoEnfermedad.Nombre = tipoEnfermedad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoEnfermedad);
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
                        Message = "Existe un tipo de enfermedad con igual nombre...",
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

        public Response Editar(TipoEnfermedad tipoEnfermedad)
        {
            try
            {
                var respuesta = Existe(tipoEnfermedad);
                if (!respuesta.IsSuccess)
                {
                    var respuestaEnfermedad = (TipoEnfermedad)respuesta.Resultado;
                    respuestaEnfermedad.Nombre = tipoEnfermedad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaEnfermedad);
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
                        Message = "Existe un tipo de enfermedad con igual nombre...",
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

        public Response Eliminar(int idEnfermedad)
        {
            try
            {
                var respuestaEnfermedad = db.TipoEnfermedad.Find(idEnfermedad);
                if (respuestaEnfermedad != null)
                {
                    db.Remove(respuestaEnfermedad);
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
                    Message = "No se encontró el tipo de enfermedad",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de enfermedad no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoEnfermedad tipoEnfermedad)
        {
            var respuestaTipoEnfermedad = db.TipoEnfermedad.Where(p => p.Nombre.ToUpper() == tipoEnfermedad.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoEnfermedad != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de enferemdad de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipo de enfermedad...",
                Resultado = db.TipoEnfermedad.Where(p => p.IdTipoEnfermedad == tipoEnfermedad.IdTipoEnfermedad).FirstOrDefault(),
            };
        }

        public TipoEnfermedad ObtenerTipoEnfermedad(int idTipoEnfermedad)
        {
            return db.TipoEnfermedad.Where(c => c.IdTipoEnfermedad == idTipoEnfermedad).FirstOrDefault();
        }

        public List<TipoEnfermedad> ObtenerTiposEnfermedades()
        {
            return db.TipoEnfermedad.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

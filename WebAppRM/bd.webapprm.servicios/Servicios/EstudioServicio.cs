 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class EstudioServicio: IEstudioServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public EstudioServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Estudio estudio)
        {
            try
            {
                var respuesta = Existe(estudio);
                if (!respuesta.IsSuccess)
                {
                    estudio.Nombre = estudio.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(estudio);
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
                        Message = "Existe un estudio con igual nombre...",
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

        public Response Editar(Estudio estudio)
        {
            try
            {
                var respuesta = Existe(estudio);
                if (!respuesta.IsSuccess)
                {
                    var respuestaEstudio = (Estudio)respuesta.Resultado;
                    respuestaEstudio.Nombre = estudio.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaEstudio);
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
                        Message = "Existe un estudio con igual nombre...",
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

        public Response Eliminar(int idEstudio)
        {
            try
            {
                var respuestaEstudio = db.Estudio.Find(idEstudio);
                if (respuestaEstudio != null)
                {
                    db.Remove(respuestaEstudio);
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
                    Message = "No se encontró el estudio",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El estudio no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Estudio estudio)
        {
            var respuestaEstudio = db.Estudio.Where(p => p.Nombre.ToUpper() == estudio.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaEstudio != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un estudio de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe estudio...",
                Resultado = db.Estudio.Where(p => p.IdEstudio == estudio.IdEstudio).FirstOrDefault(),
            };
        }

        public Estudio ObtenerEstudio(int idEstudio)
        {
            return db.Estudio.Where(c => c.IdEstudio == idEstudio).FirstOrDefault();
        }

        public List<Estudio> ObtenerEstudios()
        {
            return db.Estudio.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

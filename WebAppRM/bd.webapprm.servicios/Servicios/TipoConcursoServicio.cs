 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoConcursoServicio: ITipoConcursoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoConcursoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoConcurso tipoConcurso)
        {
            try
            {
                var respuesta = Existe(tipoConcurso);
                if (!respuesta.IsSuccess)
                {
                    tipoConcurso.Nombre = tipoConcurso.Nombre.TrimStart().TrimEnd().ToUpper();
                    tipoConcurso.Descripcion = tipoConcurso.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoConcurso);
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
                        Message = "Existe un tipo de concurso con igual nombre...",
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

        public Response Editar(TipoConcurso tipoConcurso)
        {
            try
            {
                var respuestaTipoConcurso = db.TipoConcurso.Where(x => x.IdTipoConcurso == tipoConcurso.IdTipoConcurso).FirstOrDefault();

                if (tipoConcurso.Nombre == respuestaTipoConcurso.Nombre && tipoConcurso.Descripcion == respuestaTipoConcurso.Descripcion)
                {
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "ok",
                    };
                }

                if (tipoConcurso.Nombre.ToUpper().TrimStart().TrimEnd() == respuestaTipoConcurso.Nombre.ToUpper().TrimStart().TrimEnd())
                {
                    respuestaTipoConcurso.Descripcion = tipoConcurso.Descripcion.ToUpper().TrimStart().TrimEnd();
                    db.TipoConcurso.Update(respuestaTipoConcurso);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Ok",
                        Resultado = tipoConcurso,
                    };
                }

                var respuesta = Existe(tipoConcurso);
                if (respuesta.IsSuccess)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "Existe un tipo de concurso con igual nombre",
                    };

                }

                respuestaTipoConcurso.Nombre = tipoConcurso.Nombre.TrimStart().TrimEnd().ToUpper();
                respuestaTipoConcurso.Descripcion = tipoConcurso.Descripcion.TrimStart().TrimEnd().ToUpper();
                db.TipoConcurso.Update(respuestaTipoConcurso);
                db.SaveChanges();

                return new Response
                {
                    IsSuccess = true,
                    Message = "Ok...",
                };

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

        public Response Eliminar(int idTipoConcurso)
        {
            try
            {
                var respuestaTipoConcurso = db.TipoConcurso.Find(idTipoConcurso);
                if (respuestaTipoConcurso != null)
                {
                    db.Remove(respuestaTipoConcurso);
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
                    Message = "No se encontró el tipo de concurso",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de concurso no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoConcurso tipoConcurso)
        {
            var respuestaTipoConcurso = db.TipoConcurso.Where(p => p.Nombre.ToUpper() == tipoConcurso.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoConcurso != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de concurso de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipo de concurso...",
                Resultado = db.TipoConcurso.Where(p => p.IdTipoConcurso == tipoConcurso.IdTipoConcurso).FirstOrDefault(),
            };
        }

        public TipoConcurso ObtenerTipoConcurso(int idTipoConcurso)
        {
            return db.TipoConcurso.Where(c => c.IdTipoConcurso == idTipoConcurso).FirstOrDefault();
        }

        public List<TipoConcurso> ObtenerTiposConcursos()
        {
            return db.TipoConcurso.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

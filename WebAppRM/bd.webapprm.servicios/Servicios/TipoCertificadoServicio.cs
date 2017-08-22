 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoCertificadoServicio: ITipoCertificadoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public TipoCertificadoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoCertificado tipoCertificado)
        {
            try
            {
                var respuesta = Existe(tipoCertificado);
                if (!respuesta.IsSuccess)
                {
                    tipoCertificado.Nombre = tipoCertificado.Nombre.TrimStart().TrimEnd().ToUpper();
                    tipoCertificado.Descripcion = tipoCertificado.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoCertificado);
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
                        Message = "Existe un tipo de certificado con igual nombre...",
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

        public Response Editar(TipoCertificado tipoCertificado)
        {
            try
            {
                var respuestaTipoCertificado = db.TipoCertificado.Where(x => x.IdTipoCertificado == tipoCertificado.IdTipoCertificado).FirstOrDefault();

                if (tipoCertificado.Nombre==respuestaTipoCertificado.Nombre && tipoCertificado.Descripcion==respuestaTipoCertificado.Descripcion)
                {
                    return new Response
                    {
                        IsSuccess = true,
                        Message ="ok",
                    };
                }

                if (tipoCertificado.Nombre.ToUpper().TrimStart().TrimEnd()==respuestaTipoCertificado.Nombre.ToUpper().TrimStart().TrimEnd())
                {
                    respuestaTipoCertificado.Descripcion = tipoCertificado.Descripcion;
                    db.TipoCertificado.Update(respuestaTipoCertificado);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message="Ok",
                        Resultado=tipoCertificado,
                    };
                }

                var respuesta = Existe(tipoCertificado);
                if (respuesta.IsSuccess)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "Existe un tipo de certificado con igual nombre",
                    };

                }

                respuestaTipoCertificado.Nombre = tipoCertificado.Nombre;
                respuestaTipoCertificado.Descripcion = tipoCertificado.Descripcion;
                db.TipoCertificado.Update(respuestaTipoCertificado);
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

        public Response Eliminar(int idTipoCertificado)
        {
            try
            {
                var respuestaTipoCertificado = db.TipoCertificado.Find(idTipoCertificado);
                if (respuestaTipoCertificado != null)
                {
                    db.Remove(respuestaTipoCertificado);
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
                    Message = "No se encontró el tipo de certificado",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de certificado no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoCertificado tipoCertificado)
        {
            var respuestaTipoCertificado = db.TipoCertificado.Where(p => p.Nombre.ToUpper() == tipoCertificado.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoCertificado != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de certificado de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe tipo de certificado...",
                Resultado = db.TipoCertificado.Where(p => p.IdTipoCertificado == tipoCertificado.IdTipoCertificado).FirstOrDefault(),
            };
        }

        public TipoCertificado ObtenerTipoCertificado(int idTipoCertificado)
        {
            return db.TipoCertificado.Where(c => c.IdTipoCertificado == idTipoCertificado).FirstOrDefault();
        }

        public List<TipoCertificado> ObtenerTiposCertificados()
        {
            return db.TipoCertificado.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

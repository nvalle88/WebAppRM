 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bd.webappcompartido.servicios.Servicios
{
    public class SexoServicio: ISexoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public SexoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Sexo sexo)
        {
            try
            {
                var respuesta = Existe(sexo);
                if (!respuesta.IsSuccess)
                {
                    sexo.Nombre = sexo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(sexo);
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
                        Message = "Existe un sexo con igual nombre...",
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

        public Response Editar(Sexo sexo)
        {
            try
            {
                var respuesta = Existe(sexo);
                if (!respuesta.IsSuccess)
                {
                    var respuestaSexo = (Sexo)respuesta.Resultado;
                    respuestaSexo.Nombre = sexo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaSexo);
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
                        Message = "Existe un sexo con igual nombre...",
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

        public Response Eliminar(int idSexo)
        {
            try
            {
                var respuestaSexo = db.Sexo.Find(idSexo);
                if (respuestaSexo != null)
                {
                    db.Remove(respuestaSexo);
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
                    Message = "No se encontró el sexo",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El sexo no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Sexo sexo)
        {
            var respuestaSexo = db.Sexo.Where(p => p.Nombre.ToUpper() == sexo.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaSexo != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un sexo de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el sexo...",
                Resultado = db.Sexo.Where(p => p.IdSexo == sexo.IdSexo).FirstOrDefault(),
            };
        }

        public Sexo ObtenerSexo(int idSexo)
        {
            return db.Sexo.Where(c => c.IdSexo == idSexo).FirstOrDefault();
        }

        public List<Sexo> ObtenerSexos()
        {
            return db.Sexo.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
  public  class GeneroServicio: IGeneroServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public GeneroServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Genero genero)
        {
            try
            {
                var respuesta = Existe(genero);
                if (!respuesta.IsSuccess)
                {
                    genero.Nombre = genero.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(genero);
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
                        Message = "Existe un género con igual nombre...",
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

        public Response Editar(Genero genero)
        {
            try
            {
                var respuesta = Existe(genero);
                if (!respuesta.IsSuccess)
                {
                    var respuestaGenero = (Genero)respuesta.Resultado;
                    respuestaGenero.Nombre = genero.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaGenero);
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
                        Message = "Existe un género con igual nombre...",
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

        public Response Eliminar(int iGenero)
        {
            try
            {
                var respuestaGenero = db.Genero.Find(iGenero);
                if (respuestaGenero != null)
                {
                    db.Remove(respuestaGenero);
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
                    Message = "No se encontró el género",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El género no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Genero genero)
        {
            var respuestaPais = db.Genero.Where(p => p.Nombre.ToUpper() == genero.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaPais != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe género de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe país...",
                Resultado = db.Genero.Where(p => p.IdGenero == genero.IdGenero).FirstOrDefault(),
            };
        }

        public Genero ObtenerGenero(int idGenero)
        {
            return db.Genero.Where(c => c.IdGenero == idGenero).FirstOrDefault();
        }

        public List<Genero> ObtenerGeneros()
        {
            return db.Genero.OrderBy(p => p.Nombre).ToList();
        }

   
        #endregion
    }
}

using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
using bd.webappcompartido.entidades.Utils;
 
using System.Linq;
using bd.webappcompartido.entidades;

namespace bd.webappcompartido.servicios.Servicios
{
    public class PaisServicio : IPaisServicio
    {
        #region Atributos

        
       

        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public PaisServicio(ZeusDbContext db)
        {
             
           
        }

        #endregion

        #region Metodos

        public Response Crear(Pais pais)
        {
            try
            {
                var respuesta = Existe(pais);
                if (!respuesta.IsSuccess)
                {
                    pais.Nombre = pais.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(pais);
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
                        Message = "Existe un país con igual nombre...",
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

        public Response Editar(Pais pais)
        {
            try
            {
                var respuesta = Existe(pais);
                if (!respuesta.IsSuccess)
                {
                    var respuestaPais = (Pais)respuesta.Resultado;
                    respuestaPais.Nombre = pais.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaPais);
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
                        Message = "Existe un país con igual nombre...",
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

        public Response Eliminar(int idPais)
        {
            try
            {
                var respuestaPais = db.Pais.Find(idPais);
                if (respuestaPais!=null)
                {
                    db.Remove(respuestaPais);
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
                    Message = "No se encontró el país",
                };

            }
            catch (Exception )
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El país no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Pais pais)
        {
            var respuestaPais = db.Pais.Where(p => p.Nombre.ToUpper() == pais.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaPais != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe país de igual nombre",
                    Resultado=null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe país...",
                Resultado= db.Pais.Where(p => p.IdPais == pais.IdPais).FirstOrDefault(),
            };
        }

        public Pais ObtenerPais(int idPais)
        {
            return db.Pais.Where(c => c.IdPais == idPais).FirstOrDefault();
        }

        public  List<Pais> ObtenerPaises()
        {
          return   db.Pais.OrderBy(p => p.Nombre).ToList();
        }

        public List<Pais> ObtenerPaises(int IdProvincia)
        {
           var provincia= db.Provincia.Where(p => p.IdProvincia == IdProvincia).FirstOrDefault();
            return db.Pais.Where(p=>p.IdPais==provincia.IdPais).ToList();
        }

        public List<Provincia> ObtenerProvincias(int idPais)
        {
            return  db.Provincia.Where(p => p.IdPais == idPais).ToList();
        }
        #endregion
    }
}

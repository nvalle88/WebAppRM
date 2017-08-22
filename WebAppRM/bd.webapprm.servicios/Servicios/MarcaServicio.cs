 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bd.webappcompartido.servicios.Servicios
{
    public  class MarcaServicio:IMarcaServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public MarcaServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Marca marca)
        {
            try
            {
                var respuesta = Existe(marca);
                if (!respuesta.IsSuccess)
                {
                    marca.Nombre = marca.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(marca);
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
                        Message = "Existe un marca con igual nombre...",
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

        public Response Editar(Marca marca)
        {
            try
            {
                var respuesta = Existe(marca);
                if (!respuesta.IsSuccess)
                {
                    var respuestamarca = (Marca)respuesta.Resultado;
                    respuestamarca.Nombre = marca.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestamarca);
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
                        Message = "Existe un marca con igual nombre...",
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

        public Response Eliminar(int idMarca)
        {
            try
            {
                var respuestamarca = db.Marca.Find(idMarca);
                if (respuestamarca != null)
                {
                    db.Remove(respuestamarca);
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
                    Message = "No se encontró el marca",
                };

            }
            catch (Exception )
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La marca no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Marca marca)
        {
            var respuestaPais = db.Marca.Where(p => p.Nombre.ToUpper() == marca.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaPais != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe una marca de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe Marca...",
                Resultado = db.Marca.Where(p => p.IdMarca == marca.IdMarca).FirstOrDefault(),
            };
        }

        public Marca ObtenerMarca(int idmarca)
        {
            return db.Marca.Where(c => c.IdMarca == idmarca).FirstOrDefault();
        }

        public List<Marca> ObtenerMarcas()
        {
            return db.Marca.OrderBy(p => p.Nombre).ToList();
        }
        #endregion
    }
}

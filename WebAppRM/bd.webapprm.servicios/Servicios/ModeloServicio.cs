using bd.webappcompartido.entidades.Utils;
 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{ 
   public class ModeloServicio : IModeloServicio
    {
        #region Attributes

         

        #endregion

        #region Services


        #endregion

        #region Constructors

        public ModeloServicio(ZeusDbContext db)
        {
             

        }


        #endregion

        #region Methods

        public Modelo ObtenerModelo(int IdModelo)
        {
            return db.Modelo.Where(p => p.IdModelo == IdModelo).FirstOrDefault();
        }
        public List<Modelo> ObtenerModelos()
        {
            return db.Modelo.Include(p => p.Marca).ToList();
        }

        public Response Crear(Modelo Modelo)
        {
            if (Modelo == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Modelo es Null",

                };

            }

            try
            {
                var respuesta = Existe(Modelo);

                if (respuesta.IsSuccess)
                {
                    Modelo.Nombre = Modelo.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(Modelo);
                    db.SaveChanges();

                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Modelo insertada correctamente",

                    };
                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "La modelo ya existe en el país seleccionado...",
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



        public Response Editar(Modelo Modelo)
        {
            try
            {
                var respuesta = Existe(Modelo);
                if (respuesta.IsSuccess)
                {
                    var respuestaModelo = ObtenerModelo(Modelo.IdModelo);
                    respuestaModelo.Nombre = Modelo.Nombre.TrimStart().TrimEnd().ToUpper();
                    respuestaModelo.IdMarca = Modelo.IdMarca;
                    db.Update(respuestaModelo);
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
                        Message = "La modelo ya existe en el país seleccionado...",
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

        public Response Existe(Modelo modelo)
        {
            var respuestaPais = db.Modelo.
                                   Where(p => p.Nombre.ToUpper() == modelo.Nombre.TrimStart().TrimEnd().ToUpper()
                                         && p.Marca.IdMarca == modelo.IdMarca).
                                   FirstOrDefault();
            if (respuestaPais == null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "No Existe Modelo",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "La modelo existe ...",
                Resultado = null,
            };
        }

        public Response Eliminar(int idProvincia)
        {
            try
            {
                var respuestaModelo = db.Modelo.Find(idProvincia);
                if (respuestaModelo != null)
                {
                    db.Modelo.Remove(respuestaModelo);
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
                    Message = "No se encontró la modelo",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La modelo no se puede eliminar, existen releciones que dependendientes...",
                };
            }
        }
        #endregion
    }
}

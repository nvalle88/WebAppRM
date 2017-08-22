using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
using bd.webappcompartido.entidades.Utils;
 
using System.Linq;
using Microsoft.EntityFrameworkCore;
using bd.webappcompartido.entidades;

namespace bd.webappcompartido.servicios.Servicios
{
    public class CiudadServicio : ICiudadServicio
    {
        #region Attributes

         

        #endregion

        #region Services
        
        #endregion

        #region Constructors

        public CiudadServicio(ZeusDbContext db)
        {
             
           
        }


        #endregion


        #region Methods


        public Ciudad ObtenerCiudad(int IdCiudad)
        {

            return db.Ciudad.Where(c => c.IdCiudad == IdCiudad).FirstOrDefault();

        }

        public Response Existe(Ciudad ciudad)
        {
            var provincia = db.Provincia.Find(ciudad.IdProvincia);
            var respuestaCiudad = db.Ciudad
                                  .Where(p => p.Nombre.ToUpper() == ciudad.Nombre.TrimStart().TrimEnd().ToUpper()
                                         && p.Provincia.IdProvincia == provincia.IdProvincia)
                                  .FirstOrDefault();

            if (respuestaCiudad == null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "No existe una ciudad de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "Existe una ciudad de igual nombre...",
                Resultado = null,
            };
        }


        public Response Crear(Ciudad Ciudad)
        {
            try
            {
                var respuesta = Existe(Ciudad);
                if (respuesta.IsSuccess)
                {
                    Ciudad.Nombre = Ciudad.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(Ciudad);
                    db.SaveChanges();
                    var a = Ciudad;
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
                        Message = "Existe una ciudad de igual nombre...",
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

        public Response Editar(Ciudad Ciudad)
        {
            try
            {
                var respuesta = Existe(Ciudad);
                if (respuesta.IsSuccess)
                {
                    var respuestaCiudad = ObtenerCiudad(Ciudad.IdCiudad);
                    respuestaCiudad.Nombre = Ciudad.Nombre.TrimStart().TrimEnd().ToUpper();
                    respuestaCiudad.IdProvincia = Ciudad.IdProvincia;
                    db.Ciudad.Update(respuestaCiudad);
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
                        Message = "La ciudad ya existe...",
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

        public Ciudad ObtenerCiudades(int IdCiudad)
        {
            Ciudad Ciudad = null;
            try
            {
                Ciudad = db.Ciudad.Include(b => b.Provincia).Where(b => b.IdCiudad == IdCiudad).FirstOrDefault();

            }
            catch (Exception)
            {
                Ciudad = null;
                return Ciudad;
            }
            return Ciudad;
        }

        public List<Ciudad> ObtenerCiudadades()
        {
            List<Ciudad> ciudades = null;
            try
            {
                ciudades = db.Ciudad.Include(c => c.Provincia.Pais).
                                             OrderBy(p => p.Provincia.Pais.Nombre).
                                             ThenBy(p => p.Provincia.Nombre).
                                             ThenBy(p => p.Nombre).
                                     ToList();
            }
            catch (Exception )
            {
                ciudades = null;
                return ciudades;
            }
            return ciudades;
        }

        public Provincia ObtenerProvincia(int IdCiudad)
        {
            Provincia provincia = new Provincia();
            try
            {
                var ciudad = db.Ciudad.Where(c => c.IdCiudad == IdCiudad).FirstOrDefault();
                provincia = ciudad.Provincia;

            }
            catch (Exception )
            {
                provincia = null;
                return provincia;
            }
            return provincia;
        }

        public Response Eliminar(int idCiudad)
        {
            try
            {
                var respuestaCiudad = db.Ciudad.Find(idCiudad);
                if (respuestaCiudad != null)
                {
                    db.Remove(respuestaCiudad);
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
                    Message = "No se encontró la ciudad",
                };

            }
            catch (Exception )
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La ciudad no se puede eliminar, existen releciones que dependen de la ciudad...",
                };
            }
        }

    
        #endregion
    }
}

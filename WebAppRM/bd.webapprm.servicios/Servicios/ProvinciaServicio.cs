using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
 
using System.Linq;
using Microsoft.EntityFrameworkCore;
using bd.webappcompartido.entidades.Utils;
using bd.webappcompartido.entidades;

namespace bd.webappcompartido.servicios.Servicios
{
    public class ProvinciaServicio : IProvinciaServicio
    {

        #region Attributes

         

        #endregion

        #region Services


        #endregion

        #region Constructors

        public ProvinciaServicio(ZeusDbContext db)
        {
             

        }


        #endregion

        #region Methods

        public Provincia ObtenerProvincia(int IdProvincia)
        {
            return db.Provincia.Where(p => p.IdProvincia == IdProvincia).FirstOrDefault();
        }
        public List<Provincia> ObtenerProvincias()
        {
            return db.Provincia.Include(p => p.Pais).ToList();
        }

        public Response Crear(Provincia Provincia)
        {
            if (Provincia == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Provincia es Null",

                };

            }

            try
            {
                var respuesta = Existe(Provincia);

                if (respuesta.IsSuccess)
                {
                    Provincia.Nombre = Provincia.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(Provincia);
                    db.SaveChanges();

                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Provincia insertada correctamente",

                    };
                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "La provincia ya existe en el país seleccionado...",
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



        public Response Editar(Provincia Provincia)
        {
            try
            {
                var respuesta = Existe(Provincia);
                if (respuesta.IsSuccess)
                {
                    var respuestaProvincia = ObtenerProvincia(Provincia.IdProvincia);
                    respuestaProvincia.Nombre = Provincia.Nombre.TrimStart().TrimEnd().ToUpper();
                    respuestaProvincia.IdPais = Provincia.IdPais;
                    db.Update(respuestaProvincia);
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
                        Message = "La provincia ya existe en el país seleccionado...",
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

        public Response Existe(Provincia provincia)
        {
            var respuestaPais = db.Provincia.
                                   Where(p => p.Nombre.ToUpper() == provincia.Nombre.TrimStart().TrimEnd().ToUpper()
                                         && p.Pais.IdPais == provincia.IdPais).
                                   FirstOrDefault();
            if (respuestaPais == null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "No Existe Provincia",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "La provincia existe ...",
                Resultado = null,
            };
        }

        public Response Eliminar(int idProvincia)
        {
            try
            {
                var respuestaProvincia = db.Provincia.Find(idProvincia);
                if (respuestaProvincia != null)
                {
                    db.Provincia.Remove(respuestaProvincia);
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
                    Message = "No se encontró la provincia",
                };

            }
            catch (Exception )
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La provincia no se puede eliminar, existen releciones que dependendientes...",
                };
            }
        }

        //public List<Ciudad> ObtenerCiudadades()
        //{
        //    List<Ciudad> ciudades = null;
        //    try
        //    {
        //        ciudades = db.ciudades.Include(c => c.Provincia).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        ciudades = null;
        //        return ciudades;
        //    }
        //    return ciudades;
        //}

        //public List<Parroquia> ObtenerParroquias(Ciudad Ciudad)
        //{
        //    return db.Parroquias.Where(p => p.IdCiudad == Ciudad.IdCiudad).ToList();

        //}

        //public Response CambiarSiActivo(bool esActivo,int IdProvincia)
        //{

        //    Response response = null;
        //    var Provincia = ObtenerProvincia(Convert.ToInt32(IdProvincia));


        //        try
        //        {

        //        if (esActivo)
        //        {
        //            ActiveProvince(esActivo, Provincia);
        //            response = new Response
        //            {
        //                IsSuccess = true,
        //                Message = "Ok",
        //            };
        //            db.SaveChanges();
        //            return response;

        //        }
        //        else
        //        {
        //            var ciudades = ObtenerCiudadades();

        //            foreach (var Ciudad in ciudades)
        //            {
        //                var Parroquias= ObtenerParroquias(Ciudad);
        //                ActiveCity(esActivo,Ciudad);

        //                foreach (var Parroquia in Parroquias)
        //                {
        //                    ActiveParish(esActivo,Parroquia);
        //                }
        //            }

        //            response = new Response
        //            {
        //                IsSuccess = true,
        //                Message = "Ok",
        //            };
        //            db.SaveChanges();
        //            return response;

        //        }

        //        }
        //        catch (Exception ex)
        //        {
        //            response = new Response
        //            {
        //                IsSuccess = false,
        //                Message = ex.Message,
        //            };

        //            return response;
        //        }


        //}

        //public Response ActiveParish(bool esActivo, Parroquia Parroquia)
        //{
        //    Response response = null;
        //    try
        //    {
        //        Parroquia.ParishIsActive = esActivo;
        //        db.Update(Parroquia);

        //        response = new Response
        //        {
        //            IsSuccess = true,
        //            Message = "Ok",
        //        };
        //        return response;
        //    }
        //    catch (Exception)
        //    {
        //        response = new Response
        //        {
        //            IsSuccess = false,
        //            Message = "Ok",
        //        };

        //        return response;
        //    }

        //}

        //public Response ActiveProvince(bool esActivo, Provincia Provincia)
        //{
        //    Response response = null;
        //    try
        //    {
        //        Provincia.ProvinceIsActive= esActivo;
        //        db.Update(Provincia);

        //        response = new Response
        //        {
        //            IsSuccess = true,
        //            Message = "Ok",
        //        };
        //        return response;
        //    }
        //    catch (Exception)
        //    {
        //        response = new Response
        //        {
        //            IsSuccess = false,
        //            Message = "Ok",
        //        };

        //        return response;
        //    }

        //}

        //public Response ActiveCity(bool esActivo, Ciudad Ciudad)
        //{
        //    Response response = null;
        //    try
        //    {
        //        Ciudad.CityIsActive = esActivo;
        //        db.Update(Ciudad);

        //        response = new Response
        //        {
        //            IsSuccess = true,
        //            Message = "Ok",
        //        };
        //        return response;
        //    }
        //    catch (Exception)
        //    {
        //        response = new Response
        //        {
        //            IsSuccess = false,
        //            Message = "Ok",
        //        };

        //        return response;
        //    }

        //}



        //public Response ChangeState(bool esActivo, int IdProvincia)
        //{
        //    try
        //    {
        //        var Provincia = db.Provincia.Where(p => p.IdProvincia == IdProvincia).FirstOrDefault();
        //        Provincia.ProvinceIsActive = esActivo;
        //        db.Update(Provincia);
        //        db.SaveChanges();
        //        var response = new Response
        //        {
        //            IsSuccess=true,
        //        };
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {

        //        var response = new Response
        //        {
        //            IsSuccess = false,
        //            Message= ex.Message,
        //        };
        //        return response;
        //    }
        //}



        #endregion
    }
}

 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class SucursalServicio: ISucursalServicio
    {
        #region Attributes

         

        #endregion

        #region Services


        #endregion

        #region Constructors

        public SucursalServicio(ZeusDbContext db)
        {
             

        }


        #endregion

        #region Methods

        public Sucursal ObtenerSucursal(int IdSucursal)
        {
            return db.Sucursal.Where(p => p.IdSucursal == IdSucursal).FirstOrDefault();
        }
        public List<Sucursal> ObtenerSucursales()
        {
            return db.Sucursal.Include(p => p.Ciudad).ToList();
        }

        public Response Crear(Sucursal Sucursal)
        {
            if (Sucursal == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Sucursal es Null",

                };

            }

            try
            {
                var respuesta = Existe(Sucursal);

                if (respuesta.IsSuccess)
                {
                    Sucursal.Nombre = Sucursal.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(Sucursal);
                    db.SaveChanges();

                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Sucursal insertado correctamente",

                    };
                }
                else
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "La sucursal ya existe en una ciudad...",
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



        public Response Editar(Sucursal Sucursal)
        {
            try
            {
                var respuesta = Existe(Sucursal);
                if (respuesta.IsSuccess)
                {
                    var respuestaSucursal = ObtenerSucursal(Sucursal.IdSucursal);
                    respuestaSucursal.Nombre = Sucursal.Nombre.TrimStart().TrimEnd().ToUpper();
                    respuestaSucursal.IdCiudad = Sucursal.IdCiudad;
                    db.Update(respuestaSucursal);
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
                        Message = "La sucursal ya existe en la ciudad...",
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

        public Response Existe(Sucursal sucursal)
        {
            var respuestaCiudad = db.Sucursal.
                                   Where(p => p.Nombre.ToUpper() == sucursal.Nombre.TrimStart().TrimEnd().ToUpper()).
                                   FirstOrDefault();
            if (respuestaCiudad == null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "No Existe la sucursal",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "La sucursal ya existe...",
                Resultado = null,
            };
        }

        public Response Eliminar(int idSucursal)
        {
            try
            {
                var respuestaSucursal = db.Sucursal.Find(idSucursal);
                if (respuestaSucursal != null)
                {
                    db.Sucursal.Remove(respuestaSucursal);
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
                    Message = "No se encontró la sucursal",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La sucursal no se puede eliminar, existen releciones dependendientes...",
                };
            }
        }

        //public List<Ciudad> ObtenerCiudadades()
        //{
        //    List<Ciudad> ciudades = null;
        //    try
        //    {
        //        ciudades = db.ciudades.Include(c => c.Sucursal).ToList();
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

        //public Response CambiarSiActivo(bool esActivo,int IdSucursal)
        //{

        //    Response response = null;
        //    var Sucursal = ObtenerSucursal(Convert.ToInt32(IdSucursal));


        //        try
        //        {

        //        if (esActivo)
        //        {
        //            ActiveProvince(esActivo, Sucursal);
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

        //public Response ActiveProvince(bool esActivo, Sucursal Sucursal)
        //{
        //    Response response = null;
        //    try
        //    {
        //        Sucursal.ProvinceIsActive= esActivo;
        //        db.Update(Sucursal);

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



        //public Response ChangeState(bool esActivo, int IdSucursal)
        //{
        //    try
        //    {
        //        var Sucursal = db.Sucursal.Where(p => p.IdSucursal == IdSucursal).FirstOrDefault();
        //        Sucursal.ProvinceIsActive = esActivo;
        //        db.Update(Sucursal);
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

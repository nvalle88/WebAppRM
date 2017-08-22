using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
using bd.webappcompartido.entidades.Utils;
 
using System.Linq;
using Microsoft.EntityFrameworkCore;
using bd.webappcompartido.entidades;

namespace bd.webappcompartido.servicios.Servicios
{
    public class ParroquiaService : IParroquiaServicio
    {
        #region Attributes

        ZeusDbContext db;
       ICiudadServicio CiudadServicio;
        #endregion

        #region Constructors

        public ParroquiaService(ZeusDbContext db, ICiudadServicio CiudadServicio)
        {
             
            this.CiudadServicio = CiudadServicio;
        }

        #endregion

        #region Methods
        public Response CambiarSiActivo(bool esActivo, int IdParroquia)
        {
            Response response = null;
            using (var transaccion = db.Database.BeginTransaction())
            {


                try
                {
                    if (!esActivo)
                    {
                        var Parroquia = ObtenerParroquia(IdParroquia);
                        db.Update(Parroquia);
                        response = new Response
                        {
                            IsSuccess = true,
                            Message = "Ok",
                        };
                        db.SaveChanges();
                        transaccion.Commit();

                    }
                    else
                    {
                        var Parroquia = ObtenerParroquia(IdParroquia);
                        db.Update(Parroquia);
                        var Ciudad = GetParentCity(Parroquia);
                        response = ActiveCity(Ciudad);
                        if (response.IsSuccess)
                        {
                            db.SaveChanges();
                            transaccion.Commit();
                            return response;
                        }

                        response = new Response
                        {
                            IsSuccess = false,
                            Message = "Null",
                        };
                        transaccion.Rollback();
                        return response;


                    }

                    response = new Response
                    {
                        IsSuccess = true,
                        Message="Ok",
                    };
                    return response;
                }
                catch (Exception)
                {
                    response = new Response
                    {
                        IsSuccess = false,
                        Message = "Null",
                    };
                    transaccion.Rollback();
                    return response;
                }    
            }


        }

        public Response ChangeStateParishs(bool esActivo, int IdCiudad)
        {
            throw new NotImplementedException();
        }

        public Ciudad GetParentCity(Parroquia Parroquia)
        {
            return db.Ciudad.Where(p => p.IdCiudad == Parroquia.IdCiudad).FirstOrDefault();
        }

        public Ciudad ObtenerCiudad(int IdCiudad)
        {
            return db.Ciudad.Where(c => c.IdCiudad == IdCiudad).FirstOrDefault();

        }

        public Provincia ObtenerProvincia(Ciudad Ciudad)
        {
            return db.Provincia.Where(c => c.IdProvincia == Ciudad.IdProvincia).FirstOrDefault();

        }

        public Response ActiveProvince(Provincia Provincia)
        {
            Response response = null;
            try
            {
                //Provincia.ProvinceIsActive = true;
                db.Update(Provincia);
               
                response = new Response
                {
                    IsSuccess = true,
                    Message = "Ok",
                };
                return response;
            }
            catch (Exception)
            {
                response = new Response
                {
                    IsSuccess = false,
                    Message = "Ok",
                };

                return response;
            }

        }

        public Response ActiveCity (Ciudad Ciudad)
        {
            Response response = null;
            try
            {
                
                db.Update(Ciudad);
                ActiveProvince(ObtenerProvincia(Ciudad));
                response = new Response
                {
                    IsSuccess=true,
                    Message="Ok",
                };
                return response;
            }
            catch (Exception)
            {
                response = new Response
                {
                    IsSuccess = false,
                    Message = "Ok",
                };

                return response;
            }

        }

        public Response Crear(Parroquia Parroquia)
        {
            Response response;
            try
            {
                db.Add(Parroquia);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                response = new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
                return response;
            }

            response = new Response
            {
                IsSuccess = true,
                Message = "Ok",
            };

            return response;
        }

        public Response Editar(Parroquia Parroquia)
        {
            Response response;
            try
            {
                db.Update(Parroquia);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                response = new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
                return response;
            }

            response = new Response
            {
                IsSuccess = true,
                Message = "Ok",
            };

            return response;
        }

        public Parroquia ObtenerParroquia(int IdParroquia)
        {
            Parroquia Parroquia = null;
            try
            {
                Parroquia = db.Parroquia.Where(b => b.IdParroquia == IdParroquia).FirstOrDefault();

            }
            catch (Exception )
            {
                Parroquia = null;
                return Parroquia;
            }
            return Parroquia;
        }

        public List<Parroquia> ObtenerParroquias()
        {
            List<Parroquia> Parroquia = null;
            try
            {
                Parroquia = db.Parroquia.Include(p=>p.Ciudad).ToList();
            }
            catch 
            {
                Parroquia = null;
                return Parroquia;
            }
            return Parroquia;
        }

        public List<Parroquia> ObtenerParroquias(bool esActivo)
        {
            List<Parroquia> Parroquia = null;
            //try
            //{
            //    Parroquia = db.Parroquia.Where(p=>p.ParishIsActive==esActivo).ToList();
            //}
            //catch (Exception ex)
            //{
            //    Parroquia = null;
            //    return Parroquia;
            //}
            return Parroquia;
        }
        #endregion
    }
}

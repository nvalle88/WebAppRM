using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ds.zeus.entities;
 
using System.Linq;
using bd.webappcompartido.entidades.Utils;
using bd.webappcompartido.entidades;

namespace bd.webappcompartido.servicios.Servicios
{
    public class ParentescoServicio : IParentescosServicio
    {
        #region Attributes
        ZeusDbContext db;
        #endregion

        #region Contructors
        public ParentescoServicio(ZeusDbContext db)
        {
             
        }

        public Response CambiarSiActivo(bool esActivo, int IdParentesco)
        {
            Response response;
            try
            {
                var liabily = db.Parentesco.Where(b => b.IdParentesco == IdParentesco).FirstOrDefault();
                db.Update(liabily);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                response = new Response
                {
                    IsSuccess = true,
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

        public Response Crear(Parentesco Parentesco)
        {
            try
            {
                var respuesta = Existe(Parentesco);
                if (respuesta.IsSuccess)
                {
                    db.Add(Parentesco);
                    db.SaveChanges();
                    return  new Response
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
                        Message = "Existe un parentesco con igual nombre...",
                    };
                }

                

            }
            catch (Exception ex)
            {
               return  new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public Response Editar(int id, Parentesco Parentesco)
        {
            try
            {
                var respuesta = Existe(Parentesco);
                if (respuesta.IsSuccess)
                {
                    db.Update(Parentesco);
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
                        Message = "Existe un parentesco con igual nombre...",
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
        #endregion

        #region Metodos
        public List<Parentesco> ObtenerParentescos(bool esActivo)
        {


            List<Parentesco> Parentescos = null;
            try
            {
                Parentescos = db.Parentesco.ToList();
            }
            catch (Exception)
            {
                Parentescos = null;
                return Parentescos;
            }
            return Parentescos;
        }

        public List<Parentesco> ObtenerParentescos()
        {

            List<Parentesco> Parentescos = null;
            try
            {
                Parentescos = db.Parentesco.ToList();
            }
            catch (Exception )
            {
                Parentescos = null;
                return Parentescos;
            }
            return Parentescos;
        }

        public Parentesco ObtenerParentesco(int IdParentesco)
        {
            Parentesco Parentesco = null;
            try
            {
                Parentesco = db.Parentesco.Where(b => b.IdParentesco == IdParentesco).FirstOrDefault();

            }
            catch (Exception)
            {
                Parentesco = null;
                return Parentesco;
            }
            return Parentesco;
        }

        public Response Existe(Parentesco parentesco)
        {
            var respuesta = db.Parentesco.Where(p => p.Nombre.ToUpper() == parentesco.Nombre.TrimEnd().TrimStart().ToUpper()).FirstOrDefault();
            if (respuesta!=null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Ok",
                };

            }

            return new Response
            {
                IsSuccess=false,
                Message="Error...",
            };
        }
        #endregion
    }
}

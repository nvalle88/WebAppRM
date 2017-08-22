 
using bd.webappcompartido.servicios.Interfaces;
using System;
using System.Collections.Generic;
using bd.webappcompartido.entidades;
using bd.webappcompartido.entidades.Utils;
using System.Linq;


namespace bd.webappcompartido.servicios.Servicios
{
    public class InstitucionFinancieraServicio : IInstitucionFinancieraServicio
    {
        #region Attributes
        ZeusDbContext db;
        #endregion

        #region Services

        #endregion

        #region Constructors
        public InstitucionFinancieraServicio(ZeusDbContext db)
        {
             
        }


        #endregion

        #region Methods

        public Response Existe(InstitucionFinanciera institucionFinanciera)
        {
            var respuestaInstFinanciera = db.InstitucionFinanciera.Where(p => p.Nombre.ToUpper() == institucionFinanciera.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaInstFinanciera== null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "No existe país de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "Existe una institución financiera con igual nombre...",
                Resultado = null,
            };
        }

        public Response Crear(InstitucionFinanciera institucionFinanciera)
        {
            try
            {
                var respuesta = Existe(institucionFinanciera);
                if (respuesta.IsSuccess)
                {
                    institucionFinanciera.Nombre = institucionFinanciera.Nombre.Trim().ToUpper();
                    db.Add(institucionFinanciera);
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
                        Message = "Existe una institución financiera de igual nombre...",
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

        public Response Editar(int id, InstitucionFinanciera institucionFinanciera)
        {
            try
            {
                var respuesta = Existe(institucionFinanciera);
                if (respuesta.IsSuccess)
                {
                    var respuestaInstFinanciera = ObtenerInstitucionFinanciera(id);
                    respuestaInstFinanciera.Nombre = institucionFinanciera.Nombre.Trim().TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaInstFinanciera);
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

        public InstitucionFinanciera ObtenerInstitucionFinanciera(int IdInstitucionFinanciera)
        {
            return db.InstitucionFinanciera.Where(c => c.IdInstitucionFinanciera == IdInstitucionFinanciera).FirstOrDefault();
        }

        public List<InstitucionFinanciera> ObtenerInstitucionesFinancieras()
        {
            return db.InstitucionFinanciera.OrderBy(p => p.Nombre).ToList();
        }

        public Response Eliminar(int idInstFinanciera)
        {
            try
            {
                var respuestaInstFinanciera = db.InstitucionFinanciera.Find(idInstFinanciera);
                if (respuestaInstFinanciera != null)
                {
                    db.InstitucionFinanciera.Remove(respuestaInstFinanciera);
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
                    Message = "No se encontró la institución financiera",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La institución financiera no se puede eliminar, existen releciones dependientes...",
                };
            }
        }

        #endregion
    }
}

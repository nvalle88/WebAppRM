 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
   public class PaqueteInformaticoServicio: IPaqueteInformaticoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion

        #region Constructores

        public PaqueteInformaticoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(PaquetesInformaticos paqueteInformatico)
        {
            try
            {
                var respuesta = Existe(paqueteInformatico);
                if (!respuesta.IsSuccess)
                {
                    paqueteInformatico.Nombre = paqueteInformatico.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(paqueteInformatico);
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
                        Message = "Existe un paqueteInformatico con igual nombre...",
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

        public Response Editar(PaquetesInformaticos paqueteInformatico)
        {
            try
            {
                var respuestaPaqueteInformatico = db.PaquetesInformaticos.Where(x => x.IdPaquetesInformaticos == paqueteInformatico.IdPaquetesInformaticos).FirstOrDefault();

                if (paqueteInformatico.Nombre == respuestaPaqueteInformatico.Nombre && paqueteInformatico.Descripcion == respuestaPaqueteInformatico.Descripcion)
                {
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "ok",
                    };
                }

                if (paqueteInformatico.Nombre.ToUpper().TrimStart().TrimEnd() == respuestaPaqueteInformatico.Nombre.ToUpper().TrimStart().TrimEnd())
                {
                    respuestaPaqueteInformatico.Descripcion = paqueteInformatico.Descripcion;
                    db.PaquetesInformaticos.Update(respuestaPaqueteInformatico);
                    db.SaveChanges();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = "Ok",
                        Resultado = paqueteInformatico,
                    };
                }

                var respuesta = Existe(paqueteInformatico);
                if (respuesta.IsSuccess)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = "Existe un paquete informático con igual nombre",
                    };

                }

                respuestaPaqueteInformatico.Nombre = paqueteInformatico.Nombre;
                respuestaPaqueteInformatico.Descripcion = paqueteInformatico.Descripcion;
                db.PaquetesInformaticos.Update(respuestaPaqueteInformatico);
                db.SaveChanges();

                return new Response
                {
                    IsSuccess = true,
                    Message = "Ok...",
                };

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
                var respuestamarca = db.PaquetesInformaticos.Find(idMarca);
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
                    Message = "No se encontró el paqueteInformatico",
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La paqueteInformatico no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(PaquetesInformaticos paqueteInformatico)
        {
            var respuestaPais = db.PaquetesInformaticos.Where(p => p.Nombre.ToUpper() == paqueteInformatico.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaPais != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe una paqueteInformatico de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe PaquetesInformaticos...",
                Resultado = db.PaquetesInformaticos.Where(p => p.IdPaquetesInformaticos == paqueteInformatico.IdPaquetesInformaticos).FirstOrDefault(),
            };
        }

        public PaquetesInformaticos ObtenerPaqueteInformatico(int idPaquetesInformaticos)
        {
            return db.PaquetesInformaticos.Where(c => c.IdPaquetesInformaticos == idPaquetesInformaticos).FirstOrDefault();
        }

        public List<PaquetesInformaticos> ObtenerPaquetesInformaticos()
        {
            return db.PaquetesInformaticos.OrderBy(p => p.Nombre).ToList();
        }
        #endregion
    }
}

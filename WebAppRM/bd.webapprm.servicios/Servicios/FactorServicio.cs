 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bd.webappcompartido.servicios.Servicios
{
    public class FactorServicio: IFactorServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public FactorServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Factor factor)
        {
            try
            {
                var respuesta = Existe(factor);
                if (!respuesta.IsSuccess)
                {
                    factor.Porciento = factor.Porciento;
                    db.Add(factor);
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
                        Message = "Existe un Factor con igual nombre...",
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

        public Response Editar(Factor factor)
        {
            try
            {
                var respuesta = Existe(factor);
                if (!respuesta.IsSuccess)
                {
                    var respuestaFactor = (Factor)respuesta.Resultado;
                    respuestaFactor.Porciento = factor.Porciento;
                    db.Update(respuestaFactor);
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
                        Message = "Existe un Factor con igual nombre...",
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

        public Response Eliminar(int idFactor)
        {
            try
            {
                var respuestaFactor = db.Factor.Find(idFactor);
                if (respuestaFactor != null)
                {
                    db.Remove(respuestaFactor);
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
                    Message = "No se encontró el Factor",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El Factor no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Factor factor)
        {
            var respuestaFactor = db.Factor.Where(p => p.Porciento == factor.Porciento).FirstOrDefault();
            if (respuestaFactor != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un Factor de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el Factor...",
                Resultado = db.Factor.Where(p => p.IdFactor == factor.IdFactor).FirstOrDefault(),
            };
        }

        public Factor ObtenerFactor(int idFactor)
        {
            return db.Factor.Where(c => c.IdFactor == idFactor).FirstOrDefault();
        }

        public List<Factor> ObtenerFactores()
        {
            return db.Factor.OrderBy(p => p.Porciento).ToList();
        }

        #endregion
    }
}

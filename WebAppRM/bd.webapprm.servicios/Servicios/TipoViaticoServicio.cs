using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using  bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace bd.webappcompartido.servicios.Servicios
{
    public class TipoViaticoServicio: ITipoViaticoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

       public TipoViaticoServicio()
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(TipoViatico tipoViatico)
        {
            try
            {
                var respuesta = Existe(tipoViatico);
                if (!respuesta.IsSuccess)
                {
                    tipoViatico.Descripcion = tipoViatico.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(tipoViatico);
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
                        Message = "Existe un tipo de viático con igual nombre...",
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

        public Response Editar(TipoViatico tipoViatico)
        {
            try
            {
                var respuesta = Existe(tipoViatico);
                if (!respuesta.IsSuccess)
                {
                    var respuestaTipoViatico = (TipoViatico)respuesta.Resultado;
                    respuestaTipoViatico.Descripcion = tipoViatico.Descripcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaTipoViatico);
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
                        Message = "Existe un tipo de viático con igual nombre...",
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

        public Response Eliminar(int idTipoViatico)
        {
            try
            {
                var respuestaTipoViatico = db.TipoViatico.Find(idTipoViatico);
                if (respuestaTipoViatico != null)
                {
                    db.Remove(respuestaTipoViatico);
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
                    Message = "No se encontró el tipo de viático",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El tipo de viático no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(TipoViatico tipoViatico)
        {
            var respuestaTipoViatico = db.TipoViatico.Where(p => p.Descripcion.ToUpper() == tipoViatico.Descripcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaTipoViatico != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un tipo de viático de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el tipo de viático...",
                Resultado = db.TipoViatico.Where(p => p.IdTipoViatico == tipoViatico.IdTipoViatico).FirstOrDefault(),
            };
        }

        public TipoViatico ObtenerTipoViatico(int idTipoViatico)
        {
            return db.TipoViatico.Where(c => c.IdTipoViatico == idTipoViatico).FirstOrDefault();
        }

        public List<TipoViatico> ObtenerTiposViaticos()
        {
            return db.TipoViatico.OrderBy(p => p.Descripcion).ToList();
        }

        #endregion
    }
}

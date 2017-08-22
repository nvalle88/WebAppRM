 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class EtniaServicio: IEtniaServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public EtniaServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(Etnia etnia)
        {
            try
            {
                var respuesta = Existe(etnia);
                if (!respuesta.IsSuccess)
                {
                    etnia.Nombre = etnia.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Add(etnia);
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
                        Message = "Existe una etnia con igual nombre...",
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

        public Response Editar(Etnia etnia)
        {
            try
            {
                var respuesta = Existe(etnia);
                if (!respuesta.IsSuccess)
                {
                    var respuestaEtnia = (Etnia)respuesta.Resultado;
                    respuestaEtnia.Nombre = etnia.Nombre.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaEtnia);
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
                        Message = "Existe una etnia con igual nombre...",
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

        public Response Eliminar(int idEtnia)
        {
            try
            {
                var respuestaEtnia = db.Etnia.Find(idEtnia);
                if (respuestaEtnia != null)
                {
                    db.Remove(respuestaEtnia);
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
                    Message = "No se encontró la etnia",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "La etnia no se puede eliminar, existen releciones que dependen de él...",
                };
            }
        }

        public Response Existe(Etnia etnia)
        {
            var respuestaEtnia = db.Etnia.Where(p => p.Nombre.ToUpper() == etnia.Nombre.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaEtnia != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe una etnia de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe la etnia...",
                Resultado = db.Etnia.Where(p => p.IdEtnia == etnia.IdEtnia).FirstOrDefault(),
            };
        }

        public Etnia ObtenerEtnia(int idEtnia)
        {
            return db.Etnia.Where(c => c.IdEtnia == idEtnia).FirstOrDefault();
        }

        public List<Etnia> ObtenerEtnias()
        {
            return db.Etnia.OrderBy(p => p.Nombre).ToList();
        }

        #endregion
    }
}

 
using bd.webappcompartido.entidades;
using bd.webappcompartido.servicios.Interfaces;
using bd.webappcompartido.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bd.webappcompartido.servicios.Servicios
{
    public class ItemViaticoServicio: IItemViaticoServicio
    {
        #region Atributos

         


        #endregion

        #region Servicios

        #endregion


        #region Constructores

        public ItemViaticoServicio(ZeusDbContext db)
        {
             

        }

        #endregion

        #region Metodos

        public Response Crear(ItemViatico itemViatico)
        {
            try
            {
                var respuesta = Existe(itemViatico);
                if (!respuesta.IsSuccess)
                {
                    itemViatico.Descipcion = itemViatico.Descipcion.TrimStart().TrimEnd().ToUpper();
                    db.Add(itemViatico);
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
                        Message = "Existe un item viático con igual nombre...",
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

        public Response Editar(ItemViatico itemViatico)
        {
            try
            {
                var respuesta = Existe(itemViatico);
                if (!respuesta.IsSuccess)
                {
                    var respuestaItemViatico = (ItemViatico)respuesta.Resultado;
                    respuestaItemViatico.Descipcion = itemViatico.Descipcion.TrimStart().TrimEnd().ToUpper();
                    db.Update(respuestaItemViatico);
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
                        Message = "Existe un item viático con igual nombre...",
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

        public Response Eliminar(int idItemViatico)
        {
            try
            {
                var respuestaItemViatico = db.ItemViatico.Find(idItemViatico);
                if (respuestaItemViatico != null)
                {
                    db.Remove(respuestaItemViatico);
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
                    Message = "No se encontró el item viático",
                };

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "El item viático no se puede eliminar, existen relaciones que dependen de él...",
                };
            }
        }

        public Response Existe(ItemViatico itemViatico)
        {
            var respuestaItemViatico = db.ItemViatico.Where(p => p.Descipcion.ToUpper() == itemViatico.Descipcion.TrimStart().TrimEnd().ToUpper()).FirstOrDefault();
            if (respuestaItemViatico != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un item viático de igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Message = "No existe el item viático...",
                Resultado = db.ItemViatico.Where(p => p.IdItemViatico == itemViatico.IdItemViatico).FirstOrDefault(),
            };
        }

        public ItemViatico ObtenerItemViatico(int idItemViatico)
        {
            return db.ItemViatico.Where(c => c.IdItemViatico == idItemViatico).FirstOrDefault();
        }

        public List<ItemViatico> ObtenerItemsViaticos()
        {
            return db.ItemViatico.OrderBy(p => p.Descipcion).ToList();
        }

        #endregion
    }
}

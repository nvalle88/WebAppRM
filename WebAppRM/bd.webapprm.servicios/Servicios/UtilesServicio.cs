using bd.webapprm.servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.servicios.Servicios
{
    public class UtilesServicio : IUtiles
    {
        public int? TryParseInt(object valor)
        {
            try
            {
                return int.Parse(valor.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string TryParseString(object valor)
        {
            try
            {
                string v = valor.ToString();
                return !String.IsNullOrEmpty(v) ? v : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

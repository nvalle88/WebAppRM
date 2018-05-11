using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.Utils
{
    public static class Utiles
    {
        public static int? TryParseInt(object valor)
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
    }
}

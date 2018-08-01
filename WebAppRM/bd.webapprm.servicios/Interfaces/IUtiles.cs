using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.servicios.Interfaces
{
    public interface IUtiles
    {
        int? TryParseInt(object valor);
        string TryParseString(object valor);
    }
}

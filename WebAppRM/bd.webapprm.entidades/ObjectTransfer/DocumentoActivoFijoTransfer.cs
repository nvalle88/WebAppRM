﻿using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class DocumentoActivoFijoTransfer
    {
        public string Nombre { get; set; }
        public byte[] Fichero { get; set; }
        public int? IdActivoFijo { get; set; }
        public int? IdRecepcionActivoFijoDetalle { get; set; }
    }
}
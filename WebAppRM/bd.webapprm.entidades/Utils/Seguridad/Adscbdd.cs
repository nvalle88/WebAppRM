
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bd.webapprm.entidades.Utils.Seguridad
{
    public partial class Adscbdd
    {
        public Adscbdd()
        {
            Adscgrp = new HashSet<Adscgrp>();
            Adscsist = new HashSet<Adscsist>();
        }

        public string AdbdBdd { get; set; }

        public string AdbdDescripcion { get; set; }

        public string AdbdServidor { get; set; }

        public virtual ICollection<Adscgrp> Adscgrp { get; set; }
        public virtual ICollection<Adscsist> Adscsist { get; set; }
    }
}

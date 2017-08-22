namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     

   
    public partial class MaterialApoyo
    {
        [Key]
        public int IdMaterialApoyo { get; set; }

        public string NombreDocumento { get; set; }

        public string Ubicacion { get; set; }

        public int FormularioDevengacionId { get; set; }

        public virtual FormularioDevengacion FormularioDevengacion { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class Noticia
    {
        [Key]
        public int IdNoticia { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Fecha { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Foto { get; set; }
    }
}

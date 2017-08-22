namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Canditato
    {
        [Key]
        public int IdCanditato { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        public virtual ICollection<CandidatoConcurso> CandidatoConcurso { get; set; }

        public virtual ICollection<Persona> Persona { get; set; }
    }
}

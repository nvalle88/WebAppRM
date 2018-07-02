namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MaestroArticuloSucursal
    {
        public MaestroArticuloSucursal()
        {
            InventarioArticulos = new HashSet<InventarioArticulos>();
            OrdenCompraDetalles = new HashSet<OrdenCompraDetalles>();
        }

        [Key]
        public int IdMaestroArticuloSucursal { get; set; }

        [Display(Name = "Sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdSucursal { get; set; }
        public virtual Sucursal Sucursal { get; set; }

        [Display(Name = "Art�culo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "M�nimo:")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int Minimo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "M�ximo:")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int Maximo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "C�digo de art�culo:")]
        [StringLength(5, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener n�meros.")]
        public string CodigoArticulo { get; set; }

        [NotMapped]
        public string GrupoArticulo { get; set; }

        [Display(Name = "Habilitado:")]
        public bool Habilitado { get; set; }
        
        [Display(Name = "Fecha sin existencia:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaSinExistencia { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Valor de art�culo:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorActual { get; set; }

        public virtual ICollection<InventarioArticulos> InventarioArticulos { get; set; }
        public virtual ICollection<OrdenCompraDetalles> OrdenCompraDetalles { get; set; }
    }
}
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCf.Entidades.ListaValorNs
{
    [Table("LVAL")]
    public class ListaValor : Entity<int>
    {
        [Column("TIPOLVAL")]
        public string Tipo { get; set; }
        [Column("CODLVAL")]
        public string Codigo { get; set; }
        [Column("DESCRIP")]
        public string Descripcion { get; set; }
    }
}





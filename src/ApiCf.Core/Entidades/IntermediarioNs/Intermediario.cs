using Abp.Domain.Entities;

namespace ApiCf.Entidades.IntermediarioNs
{
    public class Intermediario: Entity<int>
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    }
}



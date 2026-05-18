using Abp.Domain.Entities;
 
namespace ApiCf.Entidades.ComprobanteFiscalSecNs
{
    public class FacturaQr : Entity<int>
    { 
        public string ENCF { get; set; }
        public string IDEFACT { get; set; }
        public string NUMCOM { get; set; }

    }
}



using ApiCf.Entidades.ComprobanteFiscalSecNs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiCf.Entidades.ComprobanteFiscalSecNs
{
    public interface IComprobanteFiscalSecRepository
    {
        Task<List<ComprobanteFiscalSec>> ObtenerDatoComprobanteFiscal();
        public void ObtenerComprobanteFiscal();
    }
}





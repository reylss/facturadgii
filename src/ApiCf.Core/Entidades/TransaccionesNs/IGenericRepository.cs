using System;
using System.Threading.Tasks;

namespace ApiCf.Entidades.ListaValorNs
{
    public interface IGenericRepository
    {
        Task<DateTime> ObtenerFechaFinalApiCf();
        Task<bool> UsuarioTieneOperacionAcceso(string codigoOperacion, string codigoUsuario);
    }
}





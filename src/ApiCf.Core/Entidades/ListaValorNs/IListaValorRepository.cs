using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiCf.Entidades.ListaValorNs
{
    public interface IListaValorRepository
    {
        Task<List<ListaValor>> ObtenerListaValor(string identificador);
    }
}





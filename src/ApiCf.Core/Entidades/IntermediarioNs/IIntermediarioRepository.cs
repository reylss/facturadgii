using ApiCf.Entidades.IntermediarioNs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiCf.Entidades.IntermediarioNs
{
    public interface IIntermediarioRepository
    {
        Task<List<Intermediario>> ObtenerListaIntermediarios(string codigo = null, string descripcion = null);
    }
}





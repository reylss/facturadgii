using Abp.Domain.Services;
using System.Threading.Tasks;

namespace ApiCf.Entidades.UsuarioNs
{
    public interface IUsuarioManager : IDomainService
    {
        Task CreateAbpUser(Usuario entity, string plainPassword);
        Task<string> DesEncriptar(string encripted);

    }
        
}





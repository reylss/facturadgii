using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Runtime.Session;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.ListaValorNs;
using ApiCf.SharedNs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiCf.Entidades.UsuarioNs
{
    public class UsuarioManager : DomainService, IUsuarioManager
    {
        private readonly IListaValorRepository _listaValorManager;
        private readonly UserManager _userManager;
        private readonly IAbpSession _abpSession;
        private List<ListaValor> _politicas;
        private string _llaveSeguridad;

        public UsuarioManager(IListaValorRepository listaValorManager,
                              UserManager userManager,
                              IAbpSession abpSession)
        {
            _listaValorManager = listaValorManager;
            _userManager = userManager;
            _abpSession = abpSession;
        }


        public async Task<string> DesEncriptar(string encripted)
        {
            var cifradoAES = new CifradoAES(_listaValorManager);
            _politicas ??= await _listaValorManager.ObtenerListaValor("POLISEGU");
            _llaveSeguridad ??= _politicas.Where(x => x.Codigo == "LLAVSEGU").FirstOrDefault().Descripcion;
            return CifradoAES.Desenciptar(encripted, _llaveSeguridad);
        }

        public async Task CreateAbpUser(Usuario entity, string plainPassword)
        {

            await _userManager.InitializeOptionsAsync(_abpSession.TenantId);
            var user = await _userManager.FindByNameAsync(entity.CodigoUsuario);


            if (user == default)
            {
                user = new User()
                {
                    Name = entity.NombreUsuario,
                    Surname = ".",
                    IsActive = true,
                    EmailAddress = entity.Email,
                    NormalizedEmailAddress = entity.Email.ToUpper(),
                    UserName = entity.CodigoUsuario,
                    IsEmailConfirmed = true
                };
                user.SetNormalizedNames();

                await _userManager.CreateAsync(user, plainPassword);
            }
        }
    }
}





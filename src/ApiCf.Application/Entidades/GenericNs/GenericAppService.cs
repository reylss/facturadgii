using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.ConsultaPadronNs;
using ApiCf.Entidades.UsuarioNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiCf.Entidades.GenericNs
{
    public class GenericAppService : ApplicationService, IGenericAppService
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly UserManager _userManager;
        private readonly IUsuarioManager _usuarioManager;

        public GenericAppService(IConfiguration config,
                        IRepository<Usuario> usuarioRepository,
                        UserManager userManager,
                        IUsuarioManager usuarioManager)
        {
            _config = config;
            _usuarioRepository = usuarioRepository;
            _userManager = userManager;
            _usuarioManager = usuarioManager;
        }

    

    
    }
    public class Cedula
    {
        public string SerieId { get; set; }
        public string NumId { get; set; }
        public string Dvid { get; set; }
    }

    public class SUBGRUPO
    {
        public string Nivel { get; set; }
        public string Secuencia { get; set; }
        public string CodAplic { get; set; }
        public string Opcion { get; set; }
        public string DesAplic { get; set; }
        public string Url { get; set; }
        public List<SUBGRUPO> SubMenu { get; set; }
    }
}






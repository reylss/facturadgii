using Microsoft.CodeAnalysis.CSharp.Syntax; 
using System.Net;
using System.Text;

using ApiCf.Authentication.External;
using ApiCf.Authentication.JwtBearer;
using ApiCf.Authorization;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.UsuarioNs;
using ApiCf.Models.TokenAuth;
using ApiCf.MultiTenancy;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;

namespace ApiCf.Web.Host.Workers
{
    public class AuthToken
    {
        public static string authToken;
         
        public static void  GetAuthToken()
        {
            string codigoUsuario = "WEBTEST";
            

        }

    }
}


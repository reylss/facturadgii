using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApiCf.Authentication.External;
using ApiCf.Authentication.JwtBearer;
using ApiCf.Authorization;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.UsuarioNs;
using ApiCf.Models.TokenAuth;
using ApiCf.MultiTenancy;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiCf.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : ApiCfControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUsuarioManager _usuarioManager;
        private readonly IPasswordHasher<User> _passwordHasher;


        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IRepository<Usuario> usuarioRepository,
            IRepository<User, long> userRepository,
            IUsuarioManager usuarioManager,
            IPasswordHasher<User> passwordHasher)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _usuarioRepository = usuarioRepository;
            _userRepository = userRepository;
            _usuarioManager = usuarioManager;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }
  
        
        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var usuarioAbp = _userRepository
                                       .GetAll()
                                       .Where(x => x.UserName.ToLower() == usernameOrEmailAddress.ToLower())
                                       .FirstOrDefault();

            var usuarioExisteAbp = (usuarioAbp != null && usuarioAbp.Id > 0);
            string plainPassword = "";
            if (!usuarioExisteAbp)
            {
                var usuario = _usuarioRepository
                                .GetAll()
                                .Where(x => x.CodigoUsuario == usernameOrEmailAddress)
                                .FirstOrDefault();

                if (usuario != null)
                {
                    plainPassword = await _usuarioManager.DesEncriptar(usuario.Password);
                    await _usuarioManager.CreateAbpUser(usuario, plainPassword);
                }
            }
            else
            {
                var usuario = _usuarioRepository
                    .GetAll()
                    .Where(x => x.CodigoUsuario == usernameOrEmailAddress)
                    .FirstOrDefault();

                if (usuario != null)
                {
                    plainPassword = await _usuarioManager.DesEncriptar(usuario.Password);
                    usuarioAbp.Password = _passwordHasher.HashPassword(usuarioAbp, plainPassword);
                    await _userRepository.UpdateAsync(usuarioAbp);
                }
            }

            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken);
        }
    }
}





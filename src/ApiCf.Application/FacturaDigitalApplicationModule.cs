using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ApiCf.Authorization;

namespace ApiCf
{
    [DependsOn(
        typeof(ApiCfCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class ApiCfApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<ApiCfAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(ApiCfApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}





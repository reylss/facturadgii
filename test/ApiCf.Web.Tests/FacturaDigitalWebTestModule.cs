using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ApiCf.EntityFrameworkCore;
using ApiCf.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace ApiCf.Web.Tests
{
    [DependsOn(
        typeof(ApiCfWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class ApiCfWebTestModule : AbpModule
    {
        public ApiCfWebTestModule(ApiCfEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ApiCfWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(ApiCfWebMvcModule).Assembly);
        }
    }
}




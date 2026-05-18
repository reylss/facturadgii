using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ApiCf.Configuration;
using Abp.Threading.BackgroundWorkers;
using Abp.Timing;
using ApiCf.Timing;
using ApiCf.Workers;
using Abp;
using Abp.Events.Bus;

namespace ApiCf.Web.Host.Startup
{
    [DependsOn(
       typeof(ApiCfWebCoreModule))]
    public class ApiCfWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ApiCfWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ApiCfWebHostModule).GetAssembly());
        }
          
        
        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<TransferenciaECFWorker>());
            workManager.Add(IocManager.Resolve<ProcesarECFWorker>());
            workManager.Add(IocManager.Resolve<RespuestaECFWorker>());

        }
        
    }
}





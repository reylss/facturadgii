using Abp.Localization;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Threading.BackgroundWorkers;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using ApiCf.Authorization.Roles;
using ApiCf.Authorization.Users;
using ApiCf.Configuration;
using ApiCf.Localization;
using ApiCf.MultiTenancy;
using ApiCf.Timing;
using System.Transactions;

namespace ApiCf
{
    [DependsOn(typeof(AbpZeroCoreModule))]
    public class ApiCfCoreModule : AbpModule
    {
         
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

           
            Configuration.BackgroundJobs.IsJobExecutionEnabled = true;
            Configuration.Auditing.IsEnabled = true; //TODO: Verify
            Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;
            ApiCfLocalizationConfigurer.Configure(Configuration.Localization);


            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = ApiCfConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
            
            Configuration.Localization.Languages.Add(new LanguageInfo("fa", "فارسی", "famfamfam-flags ir"));
            
            Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = ApiCfConsts.DefaultPassPhrase;
            SimpleStringCipher.DefaultPassPhrase = ApiCfConsts.DefaultPassPhrase;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ApiCfCoreModule).GetAssembly());
        }

       
    }
}





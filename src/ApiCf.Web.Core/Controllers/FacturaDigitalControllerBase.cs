using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace ApiCf.Controllers
{
    public abstract class ApiCfControllerBase: AbpController
    {
        protected ApiCfControllerBase()
        {
            LocalizationSourceName = ApiCfConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}





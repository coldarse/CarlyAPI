using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Carly.Controllers
{
    public abstract class CarlyControllerBase: AbpController
    {
        protected CarlyControllerBase()
        {
            LocalizationSourceName = CarlyConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}

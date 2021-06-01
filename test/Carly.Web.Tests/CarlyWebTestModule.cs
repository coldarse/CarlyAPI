using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Carly.EntityFrameworkCore;
using Carly.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Carly.Web.Tests
{
    [DependsOn(
        typeof(CarlyWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class CarlyWebTestModule : AbpModule
    {
        public CarlyWebTestModule(CarlyEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CarlyWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(CarlyWebMvcModule).Assembly);
        }
    }
}
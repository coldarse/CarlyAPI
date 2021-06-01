using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Carly.Authorization;

namespace Carly
{
    [DependsOn(
        typeof(CarlyCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CarlyApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CarlyAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CarlyApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}

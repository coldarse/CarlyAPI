using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Carly.Configuration.Dto;

namespace Carly.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CarlyAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}

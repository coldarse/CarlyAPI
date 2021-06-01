using System.Threading.Tasks;
using Carly.Configuration.Dto;

namespace Carly.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}

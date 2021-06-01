using System.Threading.Tasks;
using Abp.Application.Services;
using Carly.Authorization.Accounts.Dto;

namespace Carly.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}

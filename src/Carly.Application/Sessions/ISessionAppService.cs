using System.Threading.Tasks;
using Abp.Application.Services;
using Carly.Sessions.Dto;

namespace Carly.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
